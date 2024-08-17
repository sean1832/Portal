using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Portal.Core.Compression;
using Portal.Core.Encryption;
using Portal.Gh.Common;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Utils
{
    public class BytesEncoderComponent : GH_Component
    {
        #region Metadata

        public BytesEncoderComponent()
            : base("Bytes Encoder", "EnB",
                "Encode string into bytes array",
                Config.Category, Config.SubCat.Utils)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override IEnumerable<string> Keywords => new string[] { "toBytes" };
        protected override Bitmap Icon => Icons.Encode;
        public override Guid ComponentGuid => new Guid("3d414461-a9e7-4383-b64a-eeb7af53d8d0");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "Txt", "Text to encode into binary", GH_ParamAccess.item);
            pManager.AddTextParameter("Password", "Pass", "(Optional) Encrypt bytes with a password", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Compress", "Zip", "(Optional) Compress the bytes with Gzip", GH_ParamAccess.item,
                false);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Bytes array of the text", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string txt = null;
            string password = null;
            bool compress = false;

            if (!DA.GetData(0, ref txt)) return;
            DA.GetData(1, ref password);
            DA.GetData(2, ref compress);

            byte[] bytes = Encoding.UTF8.GetBytes(txt);

            // compression
            if (compress)
            {
                int dataLength = bytes.Length;
                bytes = GZip.Compress(Encoding.UTF8.GetBytes(txt));

                float compressionRate = (float)bytes.Length / dataLength * 100; // in percentage
                // round to 2 decimal places
                compressionRate = (float)Math.Round(compressionRate, 2);
                Message = $"Compression: {compressionRate}%";
            }
            else
            {
                Message = "";
            }


            // encryption
            if (!string.IsNullOrEmpty(password))
            {
                Crypto crypto = new Crypto();
                bytes = crypto.Encrypt(bytes, password);
            }

            BytesGoo bytesGoo = new BytesGoo(bytes);

            DA.SetData(0, bytesGoo);
        }
    }
}