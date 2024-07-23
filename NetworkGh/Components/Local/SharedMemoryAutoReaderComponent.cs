using Grasshopper;
using Grasshopper.Kernel;
using NetworkGh.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Timers;
using NetworkGh.Core.SharedMemory;

namespace NetworkGh.Components.Local
{
    public class SharedMemoryAutoReaderComponent : GH_Component
    {
        private string _lastReadMessage;
        private int _lastDataSize;

        #region Metadata

        public SharedMemoryAutoReaderComponent()
            : base("Shared Memory Auto-Reader", ">>Memory<<",
                "Continuously reads data from a shared memory block at specific intervals. First 4 bytes represents size." +
                "\n\nShared Memory:\n" +
                "Enables the fastest data exchange possible by allowing direct access to a common " +
                "memory block between processes on the same machine. This method is unmatched in speed " +
                "within local settings but lacks the data integrity checks and network capabilities of the other methods.",
                Config.Category, Config.SubCat.LocalIpc)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override IEnumerable<string> Keywords => new string[] { "memory read auto", "memory auto read" };
        protected override Bitmap Icon => Icons.SharedMemoryReaderAuto;
        public override Guid ComponentGuid => new Guid("617a3089-640c-43e2-a7bd-f167afcb73a6");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Memory name", "name", "Unique identifier of a shared memory block", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Interval", "Interval", "Interval to automatically update the output (milliseconds)",
                GH_ParamAccess.item, 300);
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
            int interval = 0;
            bool read = false;
            

            if (!DA.GetData(0, ref name)) return;
            if (!DA.GetData(1, ref interval)) return;
            if (!DA.GetData(2, ref read)) return;

            if (interval <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid interval");
                return;
            }
            if (read)
            {
                try
                {
                    // Schedule the solution to run at the specified interval
                    OnPingDocument().ScheduleSolution(interval, d =>
                    {
                        ExpireSolution(true);
                    });

                    using var reader = new SharedMemoryManager(name);
                    _lastDataSize = BitConverter.ToInt32(reader.ReadRange(0, 4), 0); // read data size
                    byte[] dataBytes = reader.ReadRange(4, _lastDataSize);
                    _lastReadMessage = System.Text.Encoding.UTF8.GetString(dataBytes);
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                    _lastReadMessage = "Error";
                }
                
            }

            DA.SetData(0, _lastReadMessage);
            DA.SetData(1, _lastDataSize);
        }
    }
}