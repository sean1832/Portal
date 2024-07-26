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
    public class FileReaderComponent : GH_Component
    {
        #region Metadata

        public FileReaderComponent()
            : base("File Reader", "<File>",
                "Read a file in bytes",
                Config.Category, Config.SubCat.Local)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.FileRead;
        public override Guid ComponentGuid => new Guid("94cca7d1-589e-4a52-ab6d-82b6325cad67");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path", "path", "file path to read", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "bytes array read", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = string.Empty;

            if (!DA.GetData(0, ref path)) return;

            byte[] bytes = System.IO.File.ReadAllBytes(path);
            BytesGoo bytesGoo = new BytesGoo(bytes);

            DA.SetData(0, bytesGoo);
        }
    }
}