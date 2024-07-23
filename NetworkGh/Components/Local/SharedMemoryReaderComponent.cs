using Grasshopper;
using Grasshopper.Kernel;
using NetworkGh.Common;
using NetworkGh.Core.SharedMemory;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.MemoryMappedFiles;

namespace NetworkGh.Components.Local
{
    public class SharedMemoryReaderComponent : GH_Component
    {
        private string _lastReadMessage;
        private int _lastSize;
        #region Metadata

        public SharedMemoryReaderComponent()
            : base("Shared Memory Reader", ">Memory<",
                "Reads data once from a shared memory block. First 4 bytes represents size." +
                "\n\nShared Memory:\n" +
                "Enables the fastest data exchange possible by allowing direct access to a common " +
                "memory block between processes on the same machine. This method is unmatched in speed " +
                "within local settings but lacks the data integrity checks and network capabilities of the other methods.",
                Config.Category, Config.SubCat.LocalIpc)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
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
            pManager.AddTextParameter("Data", "Data", "Data read from the shared memory block", GH_ParamAccess.item);
            pManager.AddTextParameter("Data Size", "Size", "Incoming data size in bytes", GH_ParamAccess.item);
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
                    using var reader = new SharedMemoryManager(name);
                    _lastSize = BitConverter.ToInt32(reader.ReadRange(0, 4), 0); // read data size
                    byte[] dataBytes = reader.ReadRange(4, _lastSize);
                    _lastReadMessage = System.Text.Encoding.UTF8.GetString(dataBytes);
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                }
            }

            DA.SetData(0, _lastReadMessage);
            DA.SetData(1, _lastSize);
        }
    }
}