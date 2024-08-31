using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using Portal.Core.Binary;
using Portal.Core.DataModel;
using Portal.Core.SharedMemory;
using Portal.Gh.Common;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Local
{
    public class SharedMemoryReaderComponent : GH_Component
    {
        private byte[] _lastReadMessage = Array.Empty<byte>();
        private byte[] _lastHash;
        #region Metadata

        public SharedMemoryReaderComponent()
            : base("Shared Memory Reader", ">MMF<",
                "Reads data once from a shared memory block.\n" +
                "\n\nShared Memory:\n" +
                "Enables the fastest data exchange possible by allowing direct access to a common " +
                "memory block between processes on the same machine. This method is unmatched in speed " +
                "within local settings but lacks the data integrity checks and network capabilities of the other methods.",
                Config.Category, Config.SubCat.Local)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override IEnumerable<string> Keywords => new string[] { "memory read", "mmfreader", "mmf reader" };
        protected override Bitmap Icon => Icons.SharedMemoryReader;
        public override Guid ComponentGuid => new Guid("12ad5c40-db66-49d0-9bf7-d47c6d9a8bad");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Memory name", "Name", "Unique identifier of a shared memory block", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Data in bytes read from the shared memory block", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = null;


            if (!DA.GetData(0, ref name)) return;

            try
            {
                _lastReadMessage = ReadFromMemory(name);
            }
            catch (FileNotFoundException e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, e.Message);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
            }

            BytesGoo outputGoo = new BytesGoo(_lastReadMessage);

            DA.SetData(0, outputGoo);
        }


        private byte[] ReadFromMemory(string name)
        {
            using var smm = new SharedMemoryManager(name);
            // validate signature
            byte[] signature = smm.ReadRange(0, 2);
            try
            {
                Packet.ValidateMagicNumber(signature);
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                return Array.Empty<byte>();
            }
            
            int headerSize = PacketHeader.GetExpectedSize();
            byte[] headerBytes = smm.ReadRange(2, headerSize); // skip signature
            PacketHeader header = Packet.DeserializeHeader(headerBytes);
            int dataLength = header.Size;
            if (dataLength > 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Data Read. length: {dataLength}, mmf_name: '{name}'");
                byte[] data = smm.ReadRange(0, dataLength + headerSize + 2);
                return data;
            }
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "0 byte is read.");
            return Array.Empty<byte>();
        }
    }
}