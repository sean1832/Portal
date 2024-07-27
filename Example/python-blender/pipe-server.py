import queue
import struct
import threading

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
    print("Blender received:", data)


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

            data = win32file.ReadFile(pipe, size)[1].decode("utf-8")
            data_queue.put(data)
    except pywintypes.error:
        pass
    except Exception as e:
        print(f"Error: {e}")
        shutdown_event.set()
    finally:
        win32file.CloseHandle(pipe)


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
