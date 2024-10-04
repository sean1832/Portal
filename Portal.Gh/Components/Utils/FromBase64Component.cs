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
    public class FromBase64Component : GH_Component
    {
        public FromBase64Component()
            : base("FromBase64", "b64->b",
                "Convert a base64 string into bytes array",
                Config.Category, Config.SubCat.Utils)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override IEnumerable<string> Keywords => new string[] { "b64" };
        protected override Bitmap Icon => Icons.FromB64;
        public override Guid ComponentGuid => new Guid("d1edd931-d666-4c93-82ac-3dcc28af9fab");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Base64", "B64", "Base64 string to convert.", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "B", "Byte array", GH_ParamAccess.item);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string base64 = string.Empty;

            if (!DA.GetData(0, ref base64)) return;

            byte[] bytes = Convert.FromBase64String(base64);

            DA.SetData(0, new BytesGoo(bytes));
        }
    }
}