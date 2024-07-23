# Network GH

Network GH is a Grasshopper plugin designed to enhance Inter-Process Communication (IPC) capabilities. It allows Grasshopper to interact with various applications and services, optimizing workflows through efficient data integration across different platforms, including web-based and local network environments.

## Features

- **WebSockets**: Facilitates real-time, bidirectional communication with web applications. WebSockets are less suited for high-speed requirements but provide robust, continuous data streams, making them ideal for interactive applications requiring constant updates.

- **UDP Sockets**: Offers fast data transmission via UDP, prioritizing speed over reliability. Best for scenarios where occasional data loss is acceptable, UDP Sockets are not connection-oriented, hence faster but less reliable compared to TCP-based methods.

- **Named Pipes**: Provides reliable inter-process communication within the same machine, using stream-based data transfer. Named Pipes are highly reliable and suitable for complex data exchanges within a single local machine, but they do not support remote communication.

- **Shared Memory**: Enables the fastest data exchange possible by allowing direct access to a common memory space between processes on the same machine. This method is unmatched in speed within local settings but lacks the data integrity checks and network capabilities of the other methods.

|               | Speed | Reliability | Remote | Streamable |
| ------------- | ----- | ----------- | ------ | ---------- |
| WebSockets    | 5/10  | 9/10        | ✅      | ✅          |
| UDP Sockets   | 6/10  | 7/10        | ✅      | ❌          |
| Named Pipes   | 8/10  | 10/10       | ❌      | ✅          |
| Shared Memory | 10/10 | 6/10        | ❌      | ❌          |

