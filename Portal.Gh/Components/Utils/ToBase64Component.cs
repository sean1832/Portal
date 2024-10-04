using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Gh.Common;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Utils
{
    public class ToBase64Component : GH_Component
    {
        public ToBase64Component()
            : base("ToBase64", "b->b64",
                "Convert a byte array into base64 string.",
                Config.Category, Config.SubCat.Utils)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override IEnumerable<string> Keywords => new string[] { "b64" };
        protected override Bitmap Icon => Icons.ToB64;
        public override Guid ComponentGuid => new Guid("1c732c6a-8335-4509-b2a8-c1e68c4432f8");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "B", "Byte array to convert.", GH_ParamAccess.item);

            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Base64", "B64", "Base64 string.", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BytesGoo bytes = null;

            if (!DA.GetData(0, ref bytes)) return;

            string base64 = Convert.ToBase64String(bytes.Value);

            DA.SetData(0, base64);
        }
    }
}