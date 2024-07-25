using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Gh.Common;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Local
{
    public class FileWriterComponent : GH_Component
    {
        #region Metadata

        public FileWriterComponent()
            : base("File Writer", ">File<",
                "Write a file in bytes",
                Config.Category, Config.SubCat.Local)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("cdbb0c22-a0b5-43bd-a937-7bf7549c45e7");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "bytes to write", GH_ParamAccess.item);
            pManager.AddTextParameter("File Path", "path", "file path to write", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write", "write", "start writing", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            byte[] data = null;
            string path = string.Empty;
            bool write = false;

            if (!DA.GetData(0, ref data)) return;
            if (!DA.GetData(1, ref path)) return;
            if (!DA.GetData(2, ref write)) return;

            if (write)
            {
                System.IO.File.WriteAllBytes(path, data);
                Message = "Written";
            }
        }
    }
}