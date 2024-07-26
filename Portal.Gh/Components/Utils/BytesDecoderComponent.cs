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
    public class BytesDecoderComponent : GH_Component
    {
        #region Metadata

        public BytesDecoderComponent()
            : base("Bytes Decoder", "DeB",
                "Decode a byte array into text",
                Config.Category, Config.SubCat.Utils)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.Decode;
        public override Guid ComponentGuid => new Guid("1cb92168-973b-4d72-b4d0-907eb6c5bd61");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Byte array to decode", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "Txt", "Decoded text from binary", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BytesGoo bytes = null;

            if (!DA.GetData(0, ref bytes)) return;

            string txt = System.Text.Encoding.UTF8.GetString(bytes.Value);

            DA.SetData(0, txt);
        }
    }
}