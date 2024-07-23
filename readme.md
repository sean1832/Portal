# Network GH

Network GH is a Grasshopper plugin designed to facilitate Inter-Process Communication (IPC). It enables Grasshopper to exchange data seamlessly with other applications and services, whether they are web-based or on local networks. This plugin is particularly useful for creating integrated workflows that leverage the functionality of diverse software tools or services, enhancing productivity and data coherence.

## Features

- **WebSockets**: Establish real-time connections with web applications. This feature allows for live data exchange, making it ideal for applications that require immediate data updates and interactions.

- **UDP Sockets**: Handle data transmission over UDP protocols. Suitable for applications where speed is critical and loss of data can be tolerated, UDP Sockets provide a faster, though less reliable, alternative to TCP.

- **Named Pipes (local)**: Facilitate data exchange locally between applications on the same machine. Named Pipes are perfect for secure, reliable, and efficient communication between processes running locally.


|             | Speed | Reliability | Remote | Local | Real-time |
| ----------- | ----- | ----------- | ------ | ----- | --------- |
| WebSockets  | 3/5   | 4/5         | ✅     | ✅    | ✅        |
| UDP Sockets | 4/5   | 3/5         | ✅     | ✅    | ❌        |
| Named Pipes | 5/5   | 5/5         | ❌     | ✅    | ✅        |





