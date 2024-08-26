# Portal

Portal is a Grasshopper3D plugin designed to facilitate Inter-Process Communication (IPC), enabling seamless data exchange between Grasshopper and external applications or processes. By extending workflow capabilities beyond Grasshopper3D and Rhino3D, Portal opens up new possibilities for integrated, multi-platform design processes.

## Adaptors
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

Portal supports various communication methods, each with its own strengths:

| Method             | Speed | Reliability | Remote | Streamable | Description                                                                                                                                                                               |
| ------------------ | ----- | ----------- | ------ | ---------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| WebSockets         | 5/10  | 9/10        | ✅      | ✅          | Real-time, bidirectional communication. Ideal for interactive applications requiring constant updates.                                                                                    |
| UDP Sockets        | 6/10  | 7/10        | ✅      | ❌          | Fast data transmission, prioritizing speed over reliability. Best for scenarios where occasional data loss is acceptable.                                                                 |
| Named Pipes        | 8/10  | 10/10       | ❌      | ✅          | Reliable inter-process communication within the same machine. Suitable for complex data exchanges locally.                                                                                |
| Memory Mapped File | 10/10 | 6/10        | ❌      | ❌          | Fastest data exchange possible on the same machine. Unmatched in speed for local settings but less reliable and may cause crash or memory leak if one side doesn't handel data correctly. |
| Local File         | 1/10  | 2/10        | ❌      | ❌          | Basic method for simple data exchange. Slowest but useful for non-real-time communication.                                                                                                |

## 📊 Data Structure and Model

Portal uses a flexible data structure to facilitate communication between Grasshopper and external applications. Understanding this structure is crucial for effectively handling data transfer.

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


### Memory Mapped File and Named Pipe Structure

For Memory Mapped File and Named Pipe communication methods, the data structure is as follows:
```
[4B: int32 size] [payload]
```
- The first 4 bytes contain an `int32` value indicating the payload size.
- This prefix allows the receiver to allocate the correct memory buffer size for the incoming data.
- The payload contains the actual data bytes.

### Example Workflow

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

---

Made with ❤️ by [Zeke Zhang](https://github.com/sean1832)
