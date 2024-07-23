# Network GH

Network GH is a Grasshopper plugin designed to facilitate Inter-Process Communication (IPC). It enables Grasshopper to exchange data seamlessly with other applications and services, whether they are web-based or on local networks. This plugin is particularly useful for creating integrated workflows that leverage the functionality of diverse software tools or services, enhancing productivity and data coherence.

## Features

- **WebSockets**: Establish real-time connections with web applications. This feature allows for live data exchange, making it ideal for applications that require immediate data updates and interactions.

- **UDP Sockets**: Handle data transmission over UDP protocols. Suitable for applications where speed is critical and loss of data can be tolerated, UDP Sockets provide a faster, though less reliable, alternative to TCP.

- **Named Pipes**: Communicate with single or cross-machine applications using Named Pipes. This feature is ideal for local network communication, enabling seamless data exchange between different software tools and services.

- **Shared Memory**: Share data between processes using Shared Memory. This feature is particularly useful for high-speed data exchange between applications running on the same machine.


|               | Speed | Reliability | Remote | Streamable |
| ------------- | ----- | ----------- | ------ | ---------- |
| WebSockets    | 5/10  | 9/10        | ✅     | ✅         |
| UDP Sockets   | 6/10  | 7/10        | ✅     | ❌         |
| Named Pipes   | 8/10  | 10/10       | ❌     | ✅         |
| Shared Memory | 10/10 | 6/10        | ❌     | ❌         |


