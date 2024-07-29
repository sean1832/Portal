import gzip
import io
import json
import queue
import struct
import threading
import time

import bpy
import pywintypes
import win32file
import win32pipe
import win32event


global_pipe_server = None

class DataHandler:
    @staticmethod
    def handle_data(data):
        """Handle parsed data in string, this is the main place to implement your logic"""
        try:
            message_dics = json.loads(data)
            for i, item in enumerate(message_dics):
                vertices, faces, uvs = MeshHandler.deserialize_mesh(item)
                MeshHandler.create_or_replace_mesh(f"object_{i}", vertices, faces)
        except json.JSONDecodeError:
            print(f"Received message: {data}")


class MeshHandler:
    @staticmethod
    def deserialize_mesh(data):
        """Deserialize mesh data from json string"""
        vertices = [(v["X"], v["Y"], v["Z"]) for v in data["Vertices"]]
        faces = [tuple(face_list) for face_list in data["Faces"]]
        uvs = [(uv["X"], uv["Y"]) for uv in data["UVs"]]
        return vertices, faces, uvs

    @staticmethod
    def create_or_replace_mesh(object_name, vertices, faces):
        obj = bpy.data.objects.get(object_name)
        new_mesh_data = bpy.data.meshes.new(f"{object_name}_mesh")
        new_mesh_data.from_pydata(vertices, [], faces)
        new_mesh_data.update()

        if obj and obj.type == "MESH":
            old_mesh = obj.data
            obj.data = new_mesh_data
            bpy.data.meshes.remove(old_mesh)
        else:
            new_object = bpy.data.objects.new(object_name, new_mesh_data)
            bpy.context.collection.objects.link(new_object)

        new_mesh_data.update()


class PipeServer:
    def __init__(self):
        self.data_queue = queue.Queue()
        self.shutdown_event = threading.Event()
        self.pipe_handle = None
        self.pipe_event = None

    def handle_raw_bytes(self, pipe):
        try:
            while not self.shutdown_event.is_set():
                try:
                    size_prefix = win32file.ReadFile(pipe, 4, None)[1]
                    (size,) = struct.unpack("I", size_prefix)
                    if size == 0:
                        break

                    data = win32file.ReadFile(pipe, size, None)[1]
                    data = PipeServer.decompress_if_gzip(data).decode("utf-8")
                    self.data_queue.put(data)
                except pywintypes.error as e:
                    if e.winerror == 109:  # ERROR_BROKEN_PIPE
                        break
                    raise
        except Exception as e:
            print(f"Error in handle_raw_bytes: {e}")

    @staticmethod
    def decompress_if_gzip(data: bytes) -> bytes:
        """Check if the given byte array is gzipped and decompress it if true."""
        if data[:2] == b"\x1f\x8b":
            with gzip.GzipFile(fileobj=io.BytesIO(data)) as gz:
                try:
                    return gz.read()
                except OSError:
                    return data
        return data

    def run_server(self):
        while not self.shutdown_event.is_set():
            try:
                pipe_name = rf"\\.\pipe\{bpy.context.scene.pipe_name}"
                self.pipe_handle = win32pipe.CreateNamedPipe(
                    pipe_name,
                    win32pipe.PIPE_ACCESS_INBOUND | win32file.FILE_FLAG_OVERLAPPED,
                    win32pipe.PIPE_TYPE_MESSAGE
                    | win32pipe.PIPE_READMODE_MESSAGE
                    | win32pipe.PIPE_WAIT,
                    1,
                    65536,
                    65536,
                    0,
                    None,
                )
                self.pipe_event = win32event.CreateEvent(None, True, False, None)
                overlapped = pywintypes.OVERLAPPED()
                overlapped.hEvent = self.pipe_event

                win32pipe.ConnectNamedPipe(self.pipe_handle, overlapped)

                while not self.shutdown_event.is_set():
                    rc = win32event.WaitForSingleObject(self.pipe_event, 100)
                    if rc == win32event.WAIT_OBJECT_0:
                        self.handle_raw_bytes(self.pipe_handle)
                        win32pipe.DisconnectNamedPipe(self.pipe_handle)
                        win32pipe.ConnectNamedPipe(self.pipe_handle, overlapped)

            except pywintypes.error as e:
                if e.winerror != 233:  # Not DisconnectedNamedPipe
                    print(f"Error creating or handling pipe: {e}")
                if self.shutdown_event.is_set():
                    break
                time.sleep(1)
            finally:
                self.close_handles()

    def close_handles(self):
        if self.pipe_handle:
            win32file.CloseHandle(self.pipe_handle)
            self.pipe_handle = None
        if self.pipe_event:
            win32file.CloseHandle(self.pipe_event)
            self.pipe_event = None

    def stop_server(self):
        self.shutdown_event.set()
        if self.pipe_handle:
            try:
                win32pipe.DisconnectNamedPipe(self.pipe_handle)
            except pywintypes.error:
                pass
        if self.pipe_event:
            win32event.SetEvent(self.pipe_event)


class ModalOperator(bpy.types.Operator):
    bl_idname = "wm.modal_operator"
    bl_label = "Pipe Listener Modal Operator"

    def __init__(self):
        self._timer = None

    def modal(self, context, event):
        global global_pipe_server
        if global_pipe_server and global_pipe_server.shutdown_event.is_set():
            self.cancel(context)
            return {"CANCELLED"}

        if event.type == "TIMER":
            while global_pipe_server and not global_pipe_server.data_queue.empty():
                try:
                    data = global_pipe_server.data_queue.get_nowait()
                    DataHandler.handle_data(data)
                except queue.Empty:
                    break
        return {"PASS_THROUGH"}

    def execute(self, context):
        self._timer = context.window_manager.event_timer_add(
            context.scene.event_timer, window=context.window
        )
        context.window_manager.modal_handler_add(self)
        return {"RUNNING_MODAL"}

    def cancel(self, context):
        context.window_manager.event_timer_remove(self._timer)


class PipeServerUIPanel(bpy.types.Panel):
    bl_label = "Pipe Server Control"
    bl_idname = "PT_PipeServerControl"
    bl_space_type = "VIEW_3D"
    bl_region_type = "UI"
    bl_category = "Pipe Server"

    def draw(self, context):
        layout = self.layout
        scene = context.scene

        layout.prop(scene, "pipe_name")
        layout.prop(scene, "event_timer")

        row = layout.row()
        row.operator("wm.start_pipe_server", text="Start")
        row.operator("wm.stop_pipe_server", text="Stop")


class StartPipeServer(bpy.types.Operator):
    bl_idname = "wm.start_pipe_server"
    bl_label = "Start Pipe Server"

    def execute(self, context):
        global global_pipe_server
        if global_pipe_server is None:
            global_pipe_server = PipeServer()
        global_pipe_server.shutdown_event.clear()
        print("Starting server...")
        server_thread = threading.Thread(target=global_pipe_server.run_server, daemon=True)
        server_thread.start()
        bpy.ops.wm.modal_operator("INVOKE_DEFAULT")
        return {"FINISHED"}


class StopPipeServer(bpy.types.Operator):
    bl_idname = "wm.stop_pipe_server"
    bl_label = "Stop Pipe Server"

    def execute(self, context):
        global global_pipe_server
        if global_pipe_server:
            global_pipe_server.stop_server()
            print("Stopping server...")
            global_pipe_server = None
        return {"FINISHED"}


def register():
    bpy.utils.register_class(ModalOperator)
    bpy.utils.register_class(PipeServerUIPanel)
    bpy.utils.register_class(StartPipeServer)
    bpy.utils.register_class(StopPipeServer)
    bpy.types.Scene.pipe_name = bpy.props.StringProperty(name="Pipe Name", default="testpipe")
    bpy.types.Scene.event_timer = bpy.props.FloatProperty(
        name="Interval (seconds)", default=0.01, min=0.001, max=1.0
    )


def unregister():
    bpy.utils.unregister_class(ModalOperator)
    bpy.utils.unregister_class(PipeServerUIPanel)
    bpy.utils.unregister_class(StartPipeServer)
    bpy.utils.unregister_class(StopPipeServer)
    del bpy.types.Scene.pipe_name
    del bpy.types.Scene.event_timer


if __name__ == "__main__":
    register()
