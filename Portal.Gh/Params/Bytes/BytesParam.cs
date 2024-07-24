using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Gh.Common;

namespace Portal.Gh.Params.Bytes
{
    public class BytesParam : GH_PersistentParam<BytesGoo>
    {
        public BytesParam() :
            base("bytes", "bytes",
                "Represents a byte array",
                Config.Category, Config.SubCat.Params)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override Guid ComponentGuid => new Guid("43a5e600-6ae7-49e6-aba1-c4e8999ff48a");
        protected override GH_GetterResult Prompt_Singular(ref BytesGoo value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<BytesGoo> values)
        {
            return GH_GetterResult.cancel;
        }
    }
}