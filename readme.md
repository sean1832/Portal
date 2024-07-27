# Portal

Portal is a Grasshopper plugin designed to facilitate Inter-Process Communication (IPC). It allows Grasshopper to exchange data with external applications and processes. It is generic and can be used with any application that supports one of the communication methods provided by the plugin.

## Features
- **Data Exchange with External Applications**
- **Multiple Communication Methods**
- **Data Serialization / Deserialization**
  - Point
  - Mesh
  - Curve
    - PolylineCurve
    - ArcCurve
    - LineCurve
    - NurbsCurve
- **Data Compression / Decompression (GZip)**


## Communication
Currently supports the following communication methods:

- **UDP Sockets**: Offers fast data transmission via UDP, prioritizing speed over reliability. Best for scenarios where occasional data loss is acceptable, UDP Sockets are not connection-oriented, hence faster but less reliable compared to TCP-based methods.

- **WebSockets**: Facilitates real-time, bidirectional communication with web applications. WebSockets are less suited for high-speed requirements but provide robust, continuous data streams, making them ideal for interactive applications requiring constant updates.

- **Named Pipes**: Provides reliable inter-process communication within the same machine, using stream-based data transfer. Named Pipes are highly reliable and suitable for complex data exchanges within a single local machine, but they do not support remote communication.

- **Memory Mapped File**: Enables the fastest data exchange possible by allowing direct access to a common memory space between processes on the same machine. This method is unmatched in speed within local settings but lacks the data integrity checks and network capabilities of the other methods.

- **Local File**: Local file communication is a basic method that writes data to a file on the disk, which can be read manually or by another application. This method is the slowest and least reliable (when automated) but is useful for simple data exchange between applications that do not require real-time communication.


|                    | Speed | Reliability | Remote | Streamable |
| ------------------ | ----- | ----------- | ------ | ---------- |
| WebSockets         | 5/10  | 9/10        | ✅      | ✅          |
| UDP Sockets        | 6/10  | 7/10        | ✅      | ❌          |
| Named Pipes        | 8/10  | 10/10       | ❌      | ✅          |
| Memory Mapped File | 10/10 | 6/10        | ❌      | ❌          |
| Local File         | 1/10  | 2/10        | ❌      | ❌          |

## Examples
- [Grasshopper Implementation](./Example/grasshopper/)
- [Python Implementation](./Example/python/)

