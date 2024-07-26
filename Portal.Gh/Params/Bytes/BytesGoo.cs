using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Portal.Core.Compression;

namespace Portal.Gh.Params.Bytes
{
    public class BytesGoo : GH_Goo<byte[]>
    {
        public BytesGoo() { }

        public BytesGoo(byte[] value)
        {
            Value = value;
        }

        public override IGH_Goo Duplicate()
        {
            return new BytesGoo(Value);
        }

        public override string ToString()
        {
            string msg;

            if (Value.Length > 1024) // 1 KB
                msg =  $"{Value.Length / 1024} KB";
            else if (Value.Length > 1024*1024) // 1 MB
                msg = $"{Value.Length / (1024*1024)} MB";
            else
                msg = $"{Value.Length} B"; // 1 B

            if (GZip.IsGzipped(Value))
                msg += " (gzip)";
            return msg;
        }

        public override bool IsValid => Value != null;
        public override string TypeName => "Bytes";
        public override string TypeDescription => "Represents a bytes array";
    }
}
