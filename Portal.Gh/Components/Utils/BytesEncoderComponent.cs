using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Portal.Core.Compression;
using Portal.Core.DataModel;
using Portal.Core.Encryption;
using Portal.Core.Utils;
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
        public override Guid ComponentGuid => new Guid("01a51d94-51ea-49b8-a6ea-194dc9911c11");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "Txt", "Text to encode into binary", GH_ParamAccess.item);
            pManager.AddTextParameter("Password", "Pass", "(Optional) Encrypt bytes with a password", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Compress", "Zip", "(Optional) Compress the bytes with Gzip", GH_ParamAccess.item,
                false);
            pManager.AddBooleanParameter("Timestamp", "time", "(Optional) Add a timestamp to header to calculate elapse",
                GH_ParamAccess.item, false);
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
            bool isCompress = false;
            bool hasTimestamp = false;

            if (!DA.GetData(0, ref txt)) return;
            DA.GetData(1, ref password);
            DA.GetData(2, ref isCompress);
            DA.GetData(3, ref hasTimestamp);

            bool isEncrypted = !string.IsNullOrEmpty(password);

            byte[] payload = Encoding.UTF8.GetBytes(txt);
            Crc16 crc16 = new Crc16();
            ushort checksum = crc16.ComputeChecksum(payload);

            // compression
            if (isCompress)
            {
                int dataLength = payload.Length;
                payload = GZip.Compress(payload);

                float compressionRate = (float)payload.Length / dataLength * 100; // in percentage
                // round to 2 decimal places
                compressionRate = (float)Math.Round(compressionRate, 2);
                Message = $"Compression: {compressionRate}%";
            }
            else
            {
                Message = "";
            }

            // encryption
            if (isEncrypted)
            {
                Crypto crypto = new Crypto();
                payload = crypto.Encrypt(payload, password);
            }

            // Creating a packet with an optional timestamp
            Packet packet;
            if (hasTimestamp)
            {
                packet = new Packet(payload, isEncrypted, isCompress, Helpers.GetTimestamp(), checksum);
            }
            else
            {
                packet = new Packet(payload, isEncrypted, isCompress, checksum);
            }

            BytesGoo bytesGoo = new BytesGoo(packet.Serialize());
            DA.SetData(0, bytesGoo);
        }
    }
}