import asyncio
import datetime
import json
import threading

import websockets


# A simple data generator function to simulate data collection
def generate_data():
    import random

    return {"timestamp": datetime.datetime.now().isoformat(), "value": random.random()}


data_queue = asyncio.Queue()


def data_collector():
    count = 0
    while count < 1000:
        data = generate_data()
        asyncio.run_coroutine_threadsafe(data_queue.put(data), loop)
        count += 1


async def data_sender(websocket, path):
    try:
        while True:
            data = await data_queue.get()
            if websocket.open:
                formatted_data = json.dumps(data, indent=2)
                await websocket.send(formatted_data)
                print("Data sent:", formatted_data)
            else:
                print("WebSocket closed")
                break
    except websockets.ConnectionClosed:
        print("WebSocket closed by client")
    except Exception as e:
        print(f"Error: {e}")
    finally:
        print("Connection closed")
        if websocket.open:
            await websocket.close()
        exit(0)


async def main():
    threading.Thread(target=data_collector, daemon=True).start()
    async with websockets.serve(data_sender, "localhost", 8765):
        await asyncio.Future()  # Run indefinitely


if __name__ == "__main__":
    print("WebSocket server started at ws://localhost:8765")
    loop = asyncio.get_event_loop()
    loop.run_until_complete(main())
