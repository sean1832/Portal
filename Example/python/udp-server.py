import select
import socket


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


def main():
    ip = "127.0.0.1"
    port = 6000

    with UDPServer(ip, port) as server_socket:
        try:
            while True:
                # Using select to check for available data with a timeout interval of 1 second to handle KeyboardInterrupt
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
