# Portal Python Examples
This repository contains examples of how to connect to Portal to exchange data between python and Grasshopper through various methods.

# Requirements
- Python 3.10 or higher

# Installation
```bash
python -m venv venv
./venv/Scripts/activate
pip install -r requirements.txt
```


# Examples
## UDP Communication
UDP (User Datagram Protocol) is a connectionless protocol that is used to send data over a network. It is faster than TCP but less reliable as it does not guarantee the delivery of data. UDP is suitable for applications where speed is more important than reliability.

### Implementing UDP Communication

Client and server are two separate programs that communicate with each other using UDP. The client sends a message to the server, and the server receives the message and processes it.
<details>
<summary><h5>UDP Client (Sender)</h5></a></summary>

#### 1.a. Define `UDPClient` class
```python
class UDPClient:
    def __init__(self):
        self.sock = None

    def __enter__(self):
        try:
            self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            print("UDP socket created successfully.")
            return self.sock
        except socket.error as err:
            print(f"Failed to create socket. Error: {err}")
            raise

    def __exit__(self, exc_type, exc_val, exc_tb):
        if self.sock:
            self.sock.close()
            print("UDP socket closed.")
```

#### 1.b. Define the `main` function
```python
def main():
    """Main function to run the UDP client."""
    server_ip = "127.0.0.1"  # Replace with the server IP address
    server_port = 6000  # Replace with the server port
    message = "hey there"  # Replace with your message

    with UDPClient() as client:
        try:
            # main logic to send message
            client.sendto(message.encode("utf-8"), (server_ip, server_port))
            print(f"Message sent to {server_ip}:{server_port}")
        except socket.error as err:
            print(f"Failed to send message. Error: {err}")
            raise

if __name__ == "__main__":
    main()
```
</details>

<details>
<summary><h5>UDP Server (Listener)</h5></a></summary>

#### 1. Define `UDPServer` class
```python
class UDPServer:
    def __init__(self, ip, port):
        self.ip = ip
        self.port = port
        self.sock = None

    def __enter__(self):
        self.sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.sock.bind((self.ip, self.port))
        # Set the socket to non-blocking mode to handle KeyboardInterrupt immediately
        self.sock.setblocking(False)
        print(f"UDP Server listening on {self.ip}:{self.port}")
        return self.sock

    def __exit__(self, exc_type, exc_val, exc_tb):
        if self.sock:
            self.sock.close()
            print("Server socket has been closed.")
        return False  # Do not suppress exceptions
```

#### 2. Define the `main` function
```python
def main():
    ip = "127.0.0.1"
    port = 6000

    with UDPServer(ip, port) as server_socket:
        try:
            while True:
                # Using select to check for available data with a timeout interval of 1 second to handle KeyboardInterrupt
                # import select
                ready_to_read, _, _ = select.select([server_socket], [], [], 1)
                if ready_to_read:
                    data, addr = server_socket.recvfrom(4096)
                    if data:
                        message = data.decode("utf-8")
                        print(f"Received message from {addr}: {message}")
        except Exception as e:
            print(f"Error: {e}")
        except KeyboardInterrupt:
            print("Server shutdown requested by user.")


if __name__ == "__main__":
    main()
```

</details>


## Websocket Communication

Websockets are a communication protocol that provides full-duplex communication channels over a single TCP connection. It is more reliable than UDP and can be used for real-time communication.



## Pipe Communication

Pipes are a form of inter-process communication (IPC) that allows data to be exchanged between two processes. It is a one-way communication channel that can be used to send data from one process to another.


## Memory Mapped File Communication

Memory-mapped files are a mechanism that allows files to be mapped into memory and accessed directly from the memory. It is a form of shared memory that can be used to exchange data between processes.

