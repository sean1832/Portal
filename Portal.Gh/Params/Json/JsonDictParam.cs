// reference JSwan by andrew heumann:
// https://github.com/andrewheumann/jSwan/blob/master/jSwan/Param_JsonInput.cs
using Grasshopper.Kernel;
using Portal.Gh.Common;
using Portal.Gh.Params.Bytes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using GH_IO.Serialization;

namespace Portal.Gh.Params.Json
{
    public class JsonDictParam : GH_PersistentParam<JsonDictGoo>, IDisposable
    {
        public JsonDictParam() :
            base("Json", "Json",
                "Represents a Json Object",
                Config.Category, Config.SubCat.Params)
        {
        }
        protected override Bitmap Icon => Icons.JsonParam;
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override Guid ComponentGuid => new Guid("c5726694-d8eb-4eb4-aa55-ded1ca9da417");

        protected override GH_GetterResult Prompt_Singular(ref JsonDictGoo value)
        {
            return GH_GetterResult.cancel;
        }

        protected override GH_GetterResult Prompt_Plural(ref List<JsonDictGoo> values)
        {
            return GH_GetterResult.cancel;
        }

        public override bool Write(GH_IWriter writer)
        {
            var result = base.Write(writer);
            writer.SetInt32("ScriptParamAccess", (int)Access);
            return result;
        }


        public override bool Read(GH_IReader reader)
        {
            var result = base.Read(reader);

            if (reader.ItemExists("ScriptParamAccess"))
            {
                try
                {
                    Access = (GH_ParamAccess)reader.GetInt32("ScriptParamAccess");
                    return result;
                }
                catch (Exception ex)
                {
                    //smoosh
                }
            }
            return result;
        }

        public void Dispose()
        {
            this.ClearData();
        }
    }
}
