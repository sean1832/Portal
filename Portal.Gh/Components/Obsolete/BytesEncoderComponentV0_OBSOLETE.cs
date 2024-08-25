using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Gh.Common;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Obsolete
{
    public class BytesEncoderComponentV0_OBSOLETE : GH_Component
    {
        #region Metadata

        public BytesEncoderComponentV0_OBSOLETE()
            : base("Bytes Encoder", "EnB",
                "Encode string into bytes array",
                Config.Category, Config.SubCat.Utils)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.Encode;
        public override Guid ComponentGuid => new Guid("b32f547d-0bd2-464a-9765-2fa4af720891");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "Txt", "Text to encode into binary", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Bytes array of the text", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string txt = null;

            if (!DA.GetData(0, ref txt)) return;

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(txt);
            BytesGoo bytesGoo = new BytesGoo(bytes);

            DA.SetData(0, bytesGoo);
        }
    }
}