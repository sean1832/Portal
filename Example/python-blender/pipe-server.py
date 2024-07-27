import queue
import struct
import threading

import bpy
import pywintypes
import win32file
import win32pipe

# Thread-safe queue to communicate between the thread and Blender's main thread
data_queue = queue.Queue()


def pipe_server():
    pipe_name = r"\\.\pipe\testpipe"
    while True:
        print("Waiting for client...")
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

        try:
            win32pipe.ConnectNamedPipe(pipe, None)
            print("Client connected")
            while True:
                size_prefix = win32file.ReadFile(pipe, 4)[1]
                (size,) = struct.unpack("I", size_prefix)

                if size == 0:  # if noting is received, break the loop
                    break
                resp = win32file.ReadFile(pipe, size)
                data = resp[1].decode()
                data_queue.put(data)  # Put data into queue
        except pywintypes.error as e:
            print("Finished:", e)
        finally:
            win32file.CloseHandle(pipe)


class ModalOperator(bpy.types.Operator):
    bl_idname = "wm.modal_operator"
    bl_label = "Pipe Listener Modal Operator"

    def __init__(self):
        self._timer = None

    def modal(self, context, event):
        if event.type == "TIMER":
            # Check if there is new data in the queue
            try:
                data = data_queue.get_nowait()
                print("Blender received:", data)
                # Here you can handle the data further, e.g., update scene
            except queue.Empty:
                pass
        return {"PASS_THROUGH"}

    def execute(self, context):
        self._timer = context.window_manager.event_timer_add(0.1, window=context.window)
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
    # Start the server thread
    server_thread = threading.Thread(target=pipe_server, daemon=True)
    server_thread.start()
    # Start the modal operator
    bpy.ops.wm.modal_operator()
    bpy.ops.wm.modal_operator()
