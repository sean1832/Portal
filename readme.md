# Portal.GH
![GitHub Release](https://img.shields.io/github/v/release/sean1832/portal)
![Static Badge](https://img.shields.io/badge/Grasshopper-7.13%2B-blue)
![GitHub License](https://img.shields.io/github/license/sean1832/portal)


Portal is a Grasshopper3D plugin designed to facilitate [Inter-Process Communication (IPC)](https://en.wikipedia.org/wiki/Inter-process_communication), enabling seamless data exchange between Grasshopper and external applications or processes. By extending workflow capabilities beyond Grasshopper3D and Rhino3D, Portal opens up new possibilities for integrated, multi-platform design processes.

## 🔌Adaptors
- [Portal.blender](https://github.com/sean1832/Portal.blender)
- Portal.unity (Coming Soon)
- Portal.revit (Coming Soon)

https://github.com/user-attachments/assets/070eb40c-2fe2-4cb2-8e6d-64786fcd9897



## 🌟 Features
- **Multiple Communication Methods**:
  - UDP Sockets
  - WebSockets
  - Named Pipes
  - Memory Mapped File
  - Local File
- **Data Serialization / Deserialization** for various geometry types:
  - Point
  - Mesh
  - Curve (PolylineCurve, ArcCurve, LineCurve, NurbsCurve)
- **Data Compression / Decompression** using GZip
- **Metadata Support** for geometry (e.g., material, layer (not implemented yet) etc.)
- **Vertex Color Support** for meshes 

> [!NOTE]
> Due to the fact that I am working on this project alone in my free time, the development process maybe slow. I will try to implement features as soon as possible. If you have any feature requests, please let me know by creating a [feature request](https://github.com/sean1832/Portal/issues).

## 🗺️ Roadmap
### **0.4.0**:
- [x] Nested `Payload` data structure [#12](https://github.com/sean1832/Portal/pull/12)
- [x] Serialization of rhino viewport camera [#13](https://github.com/sean1832/Portal/pull/13)
- [x] Serialization of mesh UV [#14](https://github.com/sean1832/Portal/pull/14)
- [x] Serialization of Material, texture and object layer for referenced geometry [#14](https://github.com/sean1832/Portal/pull/14), [#15](https://github.com/sean1832/Portal/pull/15)
- [ ] Update Adaptors to support new features

### **0.5.0**:
- [feature request](https://github.com/sean1832/Portal/issues) are welcome.


## 🛠️ System Requirements

- Rhino3D 7.13+
- Windows OS

## 📥 Installation
1. Download the `Portal.Gh.zip` from the [Releases](https://github.com/sean1832/Portal/releases/latest) page.
2. Unzip and copy the `Portal.GH` folder into `...\AppData\Roaming\Grasshopper\Libraries`
3. Unblock all library files:
   - Right-click on each `.gha` and `.dll` file
   - If there's an "Unblock" option, make sure to select it

## 🔌 Communication Methods

Portal supports various communication methods, each with its own strengths and drawbacks. The choice of method depends on the specific requirements of your workflow, such as speed, reliability, and whether the communication is local or remote.
> ⭐ Recommended methods for most use cases.

| Method             | Speed | Reliability | Remote | Streamable | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| ------------------ | ----- | ----------- | ------ | ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| ⭐WebSockets        | 5/10  | 9/10        | ✅      | ✅          | [Websockets](https://en.wikipedia.org/wiki/WebSocket#:~:text=WebSocket%20is%20a%20computer%20communications,as%20RFC%206455%20in%202011.) provide a stable connection capable of handling large payloads, ideal for large, content-critical data that requires cross-machine communication over a network, but can also support local communication, although not as efficiently as Named Pipes.                                                                                                                                           |
| UDP Sockets        | 6/10  | 7/10        | ✅      | ❌          | [UDP](https://en.wikipedia.org/wiki/User_Datagram_Protocol) offers fast, connectionless transmission, ideal for payloads under **1472 bytes** requiring quick, straightforward transmission without connection setup or integrity checks (e.g., sending small instructions), and supports cross-machine communication; however, it is [not suitable for payloads over 1472 bytes](https://stackoverflow.com/questions/1098897/what-is-the-largest-safe-udp-packet-size-on-the-internet) as data may become corrupted due to fragmentation. |
| ⭐Named Pipes       | 8/10  | 10/10       | ❌      | ✅          | [Named Pipes](https://en.wikipedia.org/wiki/Named_pipe) enable unidirectional communication through a pipe, suitable for fast, reliable, and ordered byte-stream-based data transfer. Limited to the local machine.                                                                                                                                                                                                                                                                                                                        |
| Memory Mapped File | 10/10 | 6/10        | ❌      | ❌          | [Memory-Mapped Files](https://en.wikipedia.org/wiki/Memory-mapped_file) map a portion of memory to the address space of a process for efficient inter-process communication, ideal for extremely fast read and write operations directly in shared memory; however, they are limited to the local machine and may cause data corruption, memory leaks, or program crashes if not handled properly.                                                                                                                                         |
| Local File I/O     | 1/10  | 2/10        | ❌      | ❌          | File I/O operations allow reading from and writing to files on a storage device, suitable for scenarios requiring persistent storage of data; however, they are limited to the local machine and can be slow compared to memory and network operations.                                                                                                                                                                                                                                                                                    |


## ⌛Example Workflow

Here's an example of how you might send a mesh from Grasshopper to another application (e.g., Blender):

1. In Grasshopper:
   - Serialize the mesh into JSON
   - Encode the JSON text into bytes
   - Compress the bytes using GZip
   - Send the compressed data via Named Pipe

2. In the receiving application (e.g., Blender):
   - Receive the compressed bytes
   - Decompress the data
   - Decode the bytes into a string
   - Parse the JSON
   - Deserialize and construct the mesh

## ⚒️ For Developer

### Data Format

- All data is sent and received as bytes.
- Data can be compressed using GZip before sending to save bandwidth and increase transfer speed. Proper decompression handling is required on the receiving end.

### Data Types

1. **Direct Text Messages**: Simple text can be sent and received directly.

2. **Structured Data (JSON)**: Complex data structures can be sent as JSON, allowing for flexible data exchange. The receiving end must know how to handle and parse the JSON data.

3. **Geometry Data**: Geometric data (like meshes, curves) can be serialized into JSON, encoded into bytes, compressed, and then sent. The receiving end must reverse this process to reconstruct the geometry.

### Data Models

Portal provides JSON data models for various geometric entities. These models define the structure for serializing and deserializing geometric data:

- [Point Data Model](/Example/data-model/point.json)
- [Mesh Data Model](/Example/data-model/mesh.json)
- [Polyline Curve Model](/Example/data-model/polyline-curve.json)
- [Arc Curve Model](/Example/data-model/arc-curve.json)
- [Line Curve Model](/Example/data-model/line-curve.json)
- [Nurbs Curve Model](/Example/data-model/nurbs-curve.json)

### Headers
Portal uses headers to identify properties of the data being sent. These headers are used to determine the type of data being sent, the compression status, and other relevant information.

| Field        | type     | Size (bytes) | Description                                                              |
| ------------ | -------- | ------------ | ------------------------------------------------------------------------ |
| MagicNumber  | `byte[]` | 2            | Header identifier. Value: `0x70, 0x6b` (ASCII: `pk`)                     |
| isCompressed | `bool`   | 1            | Compression flag                                                         |
| isEncrypted  | `bool`   | 1            | Encryption flag                                                          |
| CRC16        | `int16`  | 2            | [CRC-16 checksum](https://en.wikipedia.org/wiki/Cyclic_redundancy_check) |
| size         | `int32`  | 4            | Payload size                                                             |
| **Total**    |          | **10**       | **Total header size**                                                    |

## 🚀 Code Examples

- [Grasshopper Implementation](./Example/grasshopper/)
- [Python Implementation](./Example/python-native/)
- **Blender Integration**: 
  - ✨ **New**: [Portal.blender](https://github.com/sean1832/Portal.blender) - A user-friendly and feature-rich Blender add-on.
  - 🗃️ **Legacy**: [Python-Blender Implementation](https://github.com/sean1832/Portal/tree/75a81188b3ee689532f92b246b4fc5bae1cfcb20/Example/python-blender) - Old script examples (compatible up to Portal.Gh [v0.1.2](https://github.com/sean1832/Portal/releases/tag/0.1.2)).
    > ⚠️ Note: The legacy implementation is outdated and not compatible with latest Portal.Gh. It's provided for reference only.


## 📜 License

Portal is licensed under the Apache License 2.0. See the [LICENSE](LICENSE) for more details.

## 📞 Support

If you encounter any issues or have questions, please file an issue on the GitHub repository.

