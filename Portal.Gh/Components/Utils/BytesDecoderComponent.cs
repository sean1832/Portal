using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using Portal.Core.Compression;
using Portal.Core.DataModel;
using Portal.Core.Encryption;
using Portal.Core.Utils;
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
        public override IEnumerable<string> Keywords => new string[] { "fromBytes", "decode bytes" };
        protected override Bitmap Icon => Icons.Decode;
        public override Guid ComponentGuid => new Guid("54513217-ed11-4cf7-af69-f0b878432f6c");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Byte array to decode", GH_ParamAccess.item);
            pManager.AddTextParameter("Password", "Pass", "(Optional) password to decrypt the message if encrypted",
                GH_ParamAccess.item);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "Txt", "Decoded text from binary", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BytesGoo bytesGoo = null;
            string password = null;

            if (!DA.GetData(0, ref bytesGoo)) return;
            DA.GetData(1, ref password);

            if (!bytesGoo.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to decode. Input invalid input bytes.");
                return;
            }

            byte[] rawData = bytesGoo.Value;
            if (rawData == null || rawData.Length == 0) return;

            Packet packet = Packet.Deserialize(rawData);
            byte[] data = packet.Data;

            // decrypt if encrypted and password is provided
            if (packet.Header.IsEncrypted)
            {
                if (string.IsNullOrEmpty(password))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to decode. Data is encrypted and password is not provided.");
                    return;
                }
                Crypto crypto = new Crypto();
                try
                {
                    data = crypto.Decrypt(data, password);
                }
                catch (CryptographicException e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to decode. Incorrect Password.");
                    return; 
                }
            }

            // decompress if gzipped
            if (packet.Header.IsCompressed)
            {
                data = GZip.Decompress(data);
            }

            // check if data is corrupted
            if (data == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to decode. Data is null.");
                return;
            }
            Crc16 crc16 = new Crc16();
            ushort checksum = crc16.ComputeChecksum(data);
            if (packet.Header.Checksum != checksum)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to decode. Data is corrupted.");
                return;
            }
            string txt = System.Text.Encoding.UTF8.GetString(data);
            DA.SetData(0, txt);
        }
    }
}