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

# Thread-safe queue to communicate between the thread and Blender's main thread
data_queue = queue.Queue()
shutdown_event = threading.Event()  # Event to signal shutdown
pipe_handle = None
pipe_event = None

def handle_data(data):
    """handle parsed data in string, this is the main place to implement your logic"""
    try:
        message_dics = json.loads(data)
        for i, item in enumerate(message_dics):
            vertices, faces, uvs = deserialize_mesh(item)
            create_or_replace_mesh(f"object_{i}", vertices, faces)

    except json.JSONDecodeError:
        print(f"Received message: {data}")


def handle_raw_bytes(pipe):
    try:
        while not shutdown_event.is_set():
            try:
                size_prefix = win32file.ReadFile(pipe, 4, None)[1]
                (size,) = struct.unpack("I", size_prefix)
                if size == 0:
                    break

                data = win32file.ReadFile(pipe, size, None)[1]
                data = decompress_if_gzip(data).decode("utf-8")
                data_queue.put(data)
            except pywintypes.error as e:
                if e.winerror == 109:  # ERROR_BROKEN_PIPE
                    break
                raise
    except Exception as e:
        print(f"Error in handle_raw_bytes: {e}")


def deserialize_mesh(data):
    """Deserialize mesh data from json string"""
    vertices = []
    faces = []
    uvs = []

    for vertex in data["Vertices"]:
        vertices.append((vertex["X"], vertex["Y"], vertex["Z"]))
    for face_list in data["Faces"]:
        faces.append(tuple(face_list))
    for uv in data["UVs"]:
        uvs.append((uv["X"], uv["Y"]))

    return vertices, faces, uvs


def decompress_if_gzip(data: bytes) -> bytes:
    """
    Check if the given byte array is gzipped and decompress it if true.
    """
    # Check for gzip magic number
    if data[:2] == b"\x1f\x8b":
        # Use gzip.GzipFile to decompress the data
        with gzip.GzipFile(fileobj=io.BytesIO(data)) as gz:
            try:
                # Decompress the data
                return gz.read()
            except OSError:
                # Return the original data if decompression fails
                return data
    else:
        # Return the original data if it's not gzipped
        return data


def create_or_replace_mesh(object_name, vertices, faces):
    # Attempt to get the object
    obj = bpy.data.objects.get(object_name)

    # Create a new mesh
    new_mesh_data = bpy.data.meshes.new(f"{object_name}_mesh")

    # Set new mesh data
    new_mesh_data.from_pydata(vertices, [], faces)
    new_mesh_data.update()

    if obj and obj.type == "MESH":
        # Replace the existing mesh data with the new mesh
        old_mesh = obj.data
        obj.data = new_mesh_data
        bpy.data.meshes.remove(old_mesh)  # Remove old mesh data
    else:
        # Create a new object and link it to the scene
        new_object = bpy.data.objects.new(object_name, new_mesh_data)
        bpy.context.collection.objects.link(new_object)

    # Update mesh with new data
    new_mesh_data.update()


def pipe_server():
    global pipe_handle, pipe_event
    while not shutdown_event.is_set():
        try:
            pipe_name = rf"\\.\pipe\{bpy.context.scene.pipe_name}"
            pipe_handle = win32pipe.CreateNamedPipe(
                pipe_name,
                win32pipe.PIPE_ACCESS_INBOUND | win32file.FILE_FLAG_OVERLAPPED,
                win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT,
                1,
                65536,
                65536,
                0,
                None,
            )
            pipe_event = win32event.CreateEvent(None, True, False, None)
            overlapped = pywintypes.OVERLAPPED()
            overlapped.hEvent = pipe_event

            win32pipe.ConnectNamedPipe(pipe_handle, overlapped)

            while not shutdown_event.is_set():
                rc = win32event.WaitForSingleObject(pipe_event, 100)  # Wait for 100ms
                if rc == win32event.WAIT_OBJECT_0:
                    handle_raw_bytes(pipe_handle)
                    # Instead of breaking, we disconnect and reconnect the pipe
                    win32pipe.DisconnectNamedPipe(pipe_handle)
                    win32pipe.ConnectNamedPipe(pipe_handle, overlapped)

            if shutdown_event.is_set():
                break

        except pywintypes.error as e:
            if e.winerror == 233: # DisconnectedNamedPipe
                pass
            else:
                print(f"Error creating or handling pipe: {e}")
            if shutdown_event.is_set():
                break
            time.sleep(1)
        finally:
            if pipe_handle:
                win32file.CloseHandle(pipe_handle)
                pipe_handle = None
            if pipe_event:
                win32file.CloseHandle(pipe_event)
                pipe_event = None


class ModalOperator(bpy.types.Operator):
    bl_idname = "wm.modal_operator"
    bl_label = "Pipe Listener Modal Operator"

    def __init__(self):
        self._timer = None

    def modal(self, context, event):
        if shutdown_event.is_set():
            self.cancel(context)
            return {"CANCELLED"}

        if event.type == "TIMER":
            while not data_queue.empty():
                try:
                    data = data_queue.get_nowait()
                    handle_data(data)
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
        # Clear the shutdown event to allow the server to run again
        shutdown_event.clear()
        print("Starting server...")
        server_thread = threading.Thread(target=pipe_server, daemon=True)
        server_thread.start()
        # Start the modal operator
        bpy.ops.wm.modal_operator("INVOKE_DEFAULT")
        return {"FINISHED"}


class StopPipeServer(bpy.types.Operator):
    bl_idname = "wm.stop_pipe_server"
    bl_label = "Stop Pipe Server"

    def execute(self, context):
        global pipe_handle, pipe_event
        shutdown_event.set()
        print("Stopping server...")

        # Force disconnect the pipe
        if pipe_handle:
            try:
                win32pipe.DisconnectNamedPipe(pipe_handle)
            except pywintypes.error:
                pass

        # Set the event to unblock WaitForSingleObject
        if pipe_event:
            win32event.SetEvent(pipe_event)

        return {"FINISHED"}


def register():
    bpy.utils.register_class(ModalOperator)
    bpy.utils.register_class(PipeServerUIPanel)
    bpy.utils.register_class(StartPipeServer)
    bpy.utils.register_class(StopPipeServer)
    bpy.types.Scene.pipe_name = bpy.props.StringProperty(name="Pipe Name", default="testpipe")
    bpy.types.Scene.event_timer = bpy.props.FloatProperty(
        name="Event Timer", default=0.01, min=0.001, max=1.0
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
