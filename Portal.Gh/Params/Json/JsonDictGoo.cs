// reference JSwan by andrew heumann:
// https://github.com/andrewheumann/jSwan/blob/master/jSwan/JDictGoo.cs
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Portal.Core.DataModel;

namespace Portal.Gh.Params.Json
{
    public class JsonDictGoo : GH_Goo<JsonDict>
    {
        public JsonDictGoo() { }

        public JsonDictGoo(JsonDict value)
        {
            this.Value = value;
        }

        public JsonDictGoo(JsonDictGoo other)
        {
            Value = other.Value;
        }

        public override IGH_Goo Duplicate()
        {
            return new JsonDictGoo(this);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool IsValid => Value != null;
        public override string TypeName => "JSON Object";
        public override string TypeDescription => "JSON object representation";
    }
}
