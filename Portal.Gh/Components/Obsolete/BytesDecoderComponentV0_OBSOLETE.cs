using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Core.Compression;
using Portal.Gh.Common;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Obsolete
{
    public class BytesDecoderComponentV0_OBSOLETE : GH_Component
    {
        #region Metadata

        public BytesDecoderComponentV0_OBSOLETE()
            : base("Bytes Decoder", "DeB",
                "Decode a byte array into text",
                Config.Category, Config.SubCat.Utils)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { "fromBytes" };
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
            BytesGoo bytesGoo = null;

            if (!DA.GetData(0, ref bytesGoo)) return;
            byte[] bytes = bytesGoo.Value;

            // decompress if gzipped
            if (GZip.IsGzipped(bytes))
            {
                bytes = GZip.Decompress(bytes);
            }
            string txt = System.Text.Encoding.UTF8.GetString(bytes);

            DA.SetData(0, txt);
        }
    }
}