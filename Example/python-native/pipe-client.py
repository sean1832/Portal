import datetime
import json
import time

import win32file


def pipe_client():
    pipe_name = r"\\.\pipe\testpipe"
    try:
        pipe = win32file.CreateFile(
            pipe_name,
            win32file.GENERIC_READ | win32file.GENERIC_WRITE,
            0,
            None,
            win32file.OPEN_EXISTING,
            0,
            None,
        )
        count = 0
        while count < 1000:
            test_message = "Hey there!"
            message_dic = {
                "message": test_message,
                "send_time": datetime.datetime.now().isoformat(),
            }
            test_message = json.dumps(message_dic)
            win32file.WriteFile(pipe, test_message.encode("utf-8"))
            print("Message sent to server")
            time.sleep(0.5)
            count += 1
    except Exception as e:
        print("Failed to communicate with pipe server:", e)
    except KeyboardInterrupt:
        print("Client is shutting down")
    finally:
        win32file.CloseHandle(pipe)


if __name__ == "__main__":
    pipe_client()
