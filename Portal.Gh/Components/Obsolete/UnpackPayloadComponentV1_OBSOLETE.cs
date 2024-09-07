using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Portal.Core.DataModel;
using Portal.Gh.Common;
using Portal.Gh.Params.Payloads;

namespace Portal.Gh.Components.Obsolete
{
    public class UnpackPayloadComponentV1_OBSOLETE : GH_Component
    {
        public UnpackPayloadComponentV1_OBSOLETE()
            : base("Unpack Payload", "Unpack",
                "Unpack a payload into data and metadata",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        #region Meta

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.UnpackPayload;
        public override Guid ComponentGuid => new Guid("dd0744b3-c8d0-42f4-af58-a017dd6a6c30");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Json String", "Txt", "Serialized JSON String", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new PayloadParam(), "Payloads", "P", "Payload packets to be deserialize", GH_ParamAccess.list);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string data = string.Empty;

            if (!DA.GetData(0, ref data)) return;

            if (!TryParseJson(data, out JArray jArray))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Not a JSON Array.");
                return;
            }

            var payload = UnpackPayload(jArray);

            DA.SetDataList(0, payload);
        }

        private bool TryParseJson(string strInput, out JArray jArray)
        {
            jArray = null;
            strInput = strInput.Trim();

            if ((strInput.StartsWith("[") && strInput.EndsWith("]")))
            {
                try
                {
                    jArray = JArray.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        private List<PayloadGoo> UnpackPayload(JArray jArray)
        {
            var payloadGoos = new List<PayloadGoo>();

            try
            {
                foreach (var item in jArray)
                {
                    var dataObj = item["Items"] as JObject;
                    var metaObj = item["Meta"] as JObject;

                    JsonDict dataDict = new JsonDict();
                    JsonDict metaDict = new JsonDict();
                    if (dataObj != null)
                    {
                        dataDict = dataObj.ToObject<JsonDict>();
                    }

                    if (metaObj != null)
                    {
                        metaDict = metaObj.ToObject<JsonDict>();
                    }

                    var payload = new PayloadGoo(new Payload(dataDict, metaDict));
                    payloadGoos.Add(payload);
                }
            }
            catch (Exception ex)
            {
                // Handle parsing error
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error processing JSON:\n {ex.Message}");
            }

            return payloadGoos;
        }
    }
}