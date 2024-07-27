import json
import queue
import struct
import threading
import gzip
import io

import bpy
import pywintypes
import win32file
import win32pipe

# Thread-safe queue to communicate between the thread and Blender's main thread
data_queue = queue.Queue()
shutdown_event = threading.Event()  # Event to signal shutdown
event_timer = 0.01  # how often to check for new data
pipe_name = r"\\.\pipe\testpipe"


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
    """handle raw bytes, this is where you parse prefix and data from the pipe"""
    try:
        win32pipe.ConnectNamedPipe(pipe, None)
        while True:

            is_open_flag = win32file.ReadFile(pipe, 1)[1]
            if is_open_flag == b"\x00":  # Close flag
                print("Close flag received, closing connection")
                shutdown_event.set()  # Signal shutdown
                break

            size_prefix = win32file.ReadFile(pipe, 4)[1]  # size is 4 bytes
            (size,) = struct.unpack("I", size_prefix)
            if size == 0:
                shutdown_event.set()  # Signal shutdown
                break

            data = win32file.ReadFile(pipe, size)[1]

            data = decompress_if_gzip(data).decode("utf-8")
            data_queue.put(data)
    except pywintypes.error:
        pass
    except Exception as e:
        print(f"Error: {e}")
        shutdown_event.set()
    finally:
        win32file.CloseHandle(pipe)


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
    print("Waiting for client...")

    while not shutdown_event.is_set():
        pipe = win32pipe.CreateNamedPipe(
            pipe_name,
            win32pipe.PIPE_ACCESS_DUPLEX,
            win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_READMODE_MESSAGE | win32pipe.PIPE_WAIT,
            1,
            65536,
            65536,
            0,
            None,
        )
        handle_raw_bytes(pipe)


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
            try:
                data = data_queue.get_nowait()
                handle_data(data)
            except queue.Empty:
                pass
        return {"PASS_THROUGH"}

    def execute(self, context):
        self._timer = context.window_manager.event_timer_add(event_timer, window=context.window)
        context.window_manager.modal_handler_add(self)
        return {"RUNNING_MODAL"}

    def cancel(self, context):
        context.window_manager.event_timer_remove(self._timer)


def register():
    bpy.utils.register_class(ModalOperator)


def unregister():
    bpy.utils.unregister_class(ModalOperator)


if __name__ == "__main__":
    register()
    server_thread = threading.Thread(target=pipe_server, daemon=True)
    server_thread.start()
    bpy.ops.wm.modal_operator()
