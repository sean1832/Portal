using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Portal.Core.SharedMemory;
using Portal.Gh.Common;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Local
{
    public class SharedMemoryWriterComponent : GH_Component
    {
        private SharedMemoryManager _smm;

        #region Metadata

        public SharedMemoryWriterComponent()
            : base("Shared Memory Writer", "<Memory>",
                "Writes data to a shared memory block. First 4 bytes represents size." +
                "\n\nShared Memory:\n" +
                "Enables the fastest data exchange possible by allowing direct access to a common " +
                "memory block between processes on the same machine. This method is unmatched in speed " +
                "within local settings but lacks the data integrity checks and network capabilities of the other methods.",
                Config.Category, Config.SubCat.Local)
        {
            Instances.DocumentServer.DocumentRemoved += OnDocumentClose;
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override IEnumerable<string> Keywords => new string[] { "memory write" };
        protected override Bitmap Icon => Icons.SharedMemoryWriter;
        public override Guid ComponentGuid => new Guid("e44d5593-6665-4396-907f-751c8bc72e03");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Memory name", "name", "Unique identifier of a shared memory block", GH_ParamAccess.item);
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Message data in bytes to write to the shared memory block", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write", "Write", "Start writing the shared memory block", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        #endregion

        #region Button

        public override void CreateAttributes()
        {
            m_attributes = new ComponentButton(this, "flush", OnFlush);
        }

        public void OnFlush()
        {
            DisposeMem();
            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Memory flushed");
            ExpireSolution(true);
        }
        #endregion

        private void OnDocumentClose(GH_DocumentServer sender, GH_Document doc)
        {
            DisposeMem();
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = null;
            BytesGoo message = null;
            bool write = false;

            if (!DA.GetData(0, ref name)) return;
            if (!DA.GetData(1, ref message)) return;
            if (!DA.GetData(2, ref write)) return;

            if (write)
            {
                try
                {
                    WriteToMemory(name, message.Value);
                }
                catch (Exception e)
                {
                    DisposeMem();
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                }
            }
        }

        private void WriteToMemory(string name, byte[] data)
        {
            _smm = new SharedMemoryManager(name);
            _smm.WriteWithSize(data);
        }

        private void DisposeMem()
        {
            _smm?.Dispose();
            _smm = null;
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            DisposeMem();
            base.RemovedFromDocument(document);
        }
    }
}