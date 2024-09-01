using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Portal.Gh.Common;

namespace Portal.Gh.Params.Payloads
{
    public class PayloadParam : GH_PersistentParam<PayloadGoo>, IDisposable
    {
        public PayloadParam() :
            base("Payload", "Payload",
                "Structured Json Packet Payload",
                Config.Category, Config.SubCat.Params)
        {
        }
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override Guid ComponentGuid => new Guid("1c9a0445-cb2b-43ac-b6fa-971c2c0b4c0a");
        protected override GH_GetterResult Prompt_Singular(ref PayloadGoo value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<PayloadGoo> values)
        {
            return GH_GetterResult.cancel;
        }

        public void Dispose()
        {
            this.ClearData();
        }
    }
}