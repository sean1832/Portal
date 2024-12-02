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
using Newtonsoft.Json.Linq;
using Portal.Core.Binary;
using Portal.Gh.Params.Json;

namespace Portal.Gh.Params.Bytes
{
    public class BytesGoo : GH_Goo<byte[]>
    {
        public bool HeaderIsValid = true;
        private PacketHeader _header;

        public BytesGoo()
        {
        }

        public BytesGoo(byte[] value)
        {
            Value = value;
            ProcessHeader();
        }

        private void ProcessHeader()
        {
            if (Value == null || Value.Length == 0)
            {
                HeaderIsValid = false;
                return;
            }

            try
            {
                Packet.ValidateMagicNumber(Value);
                _header = Packet.DeserializeHeader(Value, 2);
                HeaderIsValid = _header != null;
            }
            catch (Exception)
            {
                HeaderIsValid = false;
            }
        }


        public override IGH_Goo Duplicate()
        {
            var cloned = new BytesGoo(Value);
            cloned.HeaderIsValid = HeaderIsValid; // Ensure the validity flag is copied
            return cloned;
        }

        public override string ToString()
        {
            if (Value == null || Value.Length == 0)
            {
                return "0 B";
            }

            string msg = FormatByteSize(Value.Length);

            if (!HeaderIsValid)
            {
                return $"Invalid header | {msg}";
            }

            if (_header == null)
            {
                ProcessHeader();
            }

            // re-read the header
            if (_header.IsCompressed)
            {
                msg = $"gzip({msg})";
            }

            if (_header.IsEncrypted)
            {
                msg = $"AES({msg})";
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
                    return $"{(float)length / threshold.Key} {threshold.Value}";
                }
            }

            // fallback to bytes
            return $"{length} B";
        }

        public override bool CastFrom(object source)
        {

            if (GH_Convert.ToString(source, out var str, GH_Conversion.Primary))
            {
                try
                {
                    byte[] rawBytes = Encoding.UTF8.GetBytes(str);
                    Packet packet = new Packet(rawBytes, false, false, new Crc16().ComputeChecksum(rawBytes));
                    Value = packet.Serialize();
                    HeaderIsValid = true;
                }
                catch (Exception e)
                {
                    HeaderIsValid = false;
                }
                
                return true;
            }

            if (source is JsonDictGoo jsonGoo)
            {
                var json = jsonGoo.Value;
                try
                {
                    byte[] rawBytes = Encoding.UTF8.GetBytes(json.ToString(true));
                    Packet packet = new Packet(rawBytes, false, false, new Crc16().ComputeChecksum(rawBytes));
                    Value = packet.Serialize();
                    HeaderIsValid = true;
                }
                catch (Exception e)
                {
                    HeaderIsValid = false;
                }
                return true;
            }

            return false;
        }

        
        public override bool IsValid
        {
            //get { return Value != null && _headerIsValid; }
            get { return Value != null; }
        }

        public override string TypeName => "Bytes";
        public override string TypeDescription => "Represents a bytes array";
    }
}
