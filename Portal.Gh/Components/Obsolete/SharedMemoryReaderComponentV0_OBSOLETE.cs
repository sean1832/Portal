using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using Portal.Core.SharedMemory;
using Portal.Gh.Common;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Obsolete
{
    public class SharedMemoryReaderComponentV0_OBSOLETE : GH_Component
    {
        private byte[] _lastReadMessage = Array.Empty<byte>();
        private int _lastSize;
        #region Metadata

        public SharedMemoryReaderComponentV0_OBSOLETE()
            : base("Shared Memory Reader", ">Memory<",
                "Reads data once from a shared memory block.\n" +
                "[4b: int32 size] [data]" +
                "\n\nShared Memory:\n" +
                "Enables the fastest data exchange possible by allowing direct access to a common " +
                "memory block between processes on the same machine. This method is unmatched in speed " +
                "within local settings but lacks the data integrity checks and network capabilities of the other methods.",
                Config.Category, Config.SubCat.Local)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { "memory read" };
        protected override Bitmap Icon => Icons.SharedMemoryReader;
        public override Guid ComponentGuid => new Guid("1d19b1bb-caa6-45e1-8131-542d8e22f7dc");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Memory name", "name", "Unique identifier of a shared memory block", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Start", "Start", "Start reading the shared memory block", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Data in bytes read from the shared memory block", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = null;
            bool read = false;


            if (!DA.GetData(0, ref name)) return;
            if (!DA.GetData(1, ref read)) return;

            if (read)
            {
                try
                {
                    _lastReadMessage = ReadFromMemory(name);
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                }
            }

            BytesGoo outputGoo = new BytesGoo(_lastReadMessage);

            DA.SetData(0, outputGoo);
        }


        private byte[] ReadFromMemory(string name)
        {
            using var smm = new SharedMemoryManager(name);
            byte[] lengthBuffer = smm.ReadRange(0, 4);
            int dataLength = BitConverter.ToInt32(lengthBuffer, 0);
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, $"Data Read. length:{dataLength}, mmap_filename: '{name}'");
            if (dataLength > 0)
            {
                byte[] data = smm.ReadRange(4, dataLength);
                return data;
            }
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No data read.");
            return Array.Empty<byte>();
        }
    }
}