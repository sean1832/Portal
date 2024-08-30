using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Portal.Core.Compression;
using Portal.Core.DataModel;
using Portal.Core.Encryption;
using Portal.Core.Utils;

namespace Portal.Gh.Params.Bytes
{
    public class BytesGoo : GH_Goo<byte[]>
    {
        public BytesGoo() { }

        public BytesGoo(byte[] value)
        {
            Value = value;
        }

        //// convert string to bytes
        //public BytesGoo(string value)
        //{
        //    Value = Encoding.UTF8.GetBytes(value);
        //}

        public override IGH_Goo Duplicate()
        {
            return new BytesGoo(Value);
        }

        public override string ToString()
        {
            string msg;
            if (Value != null && Value.Length != 0) {
                msg = FormatByteSize(Value.Length);
            } else {
                msg = "0 B";
                return msg;
            }

            PacketHeader header = Packet.DeserializeHeader(Value);
            if (header != null)
            {
                if (header.IsCompressed)
                {
                    msg = $"gzip({msg})";
                }
                if (header.IsEncrypted)
                {
                    msg = $"AES({msg})";
                }
            }
            else
            {
                msg = $"Invalid header | {msg}";
            }
            return msg;
        }

        private string FormatByteSize(int length)
        {
            var thresholds = new Dictionary<long, string>
            {
                { 1024 * 1024, "MB" },
                { 1024, "KB" },
                { 1, "B" }
            };
            foreach (var threshold in thresholds)
            {
                if (length >= threshold.Key)
                {
                    return $"{length / threshold.Key} {threshold.Value}";
                }
            }

            // fallback to bytes
            return $"{length} B";
        }

        public override bool CastFrom(object source)
        {

            if (GH_Convert.ToString(source, out var str, GH_Conversion.Primary))
            {
                byte[] rawBytes = Encoding.UTF8.GetBytes(str);
                ushort checksum = new Crc16().ComputeChecksum(rawBytes);
                Packet packet = new Packet(rawBytes, false, false, checksum);
                Value = packet.Serialize();
                return true;
            }

            return false;
        }

        public override bool IsValid => Value != null;
        public override string TypeName => "Bytes";
        public override string TypeDescription => "Represents a bytes array";
    }
}
