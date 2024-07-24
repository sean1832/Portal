using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return $"{Value.Length} bytes";
        }

        public override bool IsValid => Value != null;
        public override string TypeName => "Bytes";
        public override string TypeDescription => "Represents a bytes array";
    }
}
