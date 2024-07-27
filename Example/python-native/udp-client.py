import socket


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


def main():
    """Main function to run the UDP client."""
    server_ip = "127.0.0.1"  # Replace with the server IP address
    server_port = 6000  # Replace with the server port
    message = "hey there"  # Replace with your message

    with UDPClient() as client:
        try:
            client.sendto(message.encode("utf-8"), (server_ip, server_port))
            print(f"Message sent to {server_ip}:{server_port}")
        except socket.error as err:
            print(f"Failed to send message. Error: {err}")
            raise


if __name__ == "__main__":
    main()
