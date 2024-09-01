using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Portal.Core.DataModel;

namespace Portal.Gh.Params.Payloads
{
    public class PayloadGoo : GH_Goo<Payload>
    {
        public PayloadGoo() { }

        public PayloadGoo(Payload value)
        {
            this.Value = value;
        }

        public PayloadGoo(PayloadGoo other)
        {
            this.Value = other.Value;
        }

        public override IGH_Goo Duplicate()
        {
            return new PayloadGoo(this);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool IsValid => Value != null;
        public override string TypeName => "Payload";
        public override string TypeDescription => "Json Packet Payload";
    }
}
