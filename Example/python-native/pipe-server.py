import threading

import pywintypes
import win32file
import win32pipe


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
                resp = win32file.ReadFile(pipe, 64 * 1024)
                print("Received: ", resp[1].decode())
        except pywintypes.error as e:
            print("Finished:", e)
        except KeyboardInterrupt:
            print("server is shutting down")
            break
        finally:
            win32file.CloseHandle(pipe)


if __name__ == "__main__":
    server_thread = threading.Thread(target=pipe_server)
    server_thread.start()
