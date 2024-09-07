using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Portal.Core.DataModel;
using Portal.Gh.Common;
using Portal.Gh.Params.Json;
using System.Linq;
using Portal.Gh.Params.Payloads;

namespace Portal.Gh.Components.Serialization
{
    public class UnpackPayloadComponent : GH_Component
    {
        public UnpackPayloadComponent()
            : base("Unpack Payload", "Unpack",
                "Unpack a payload into data and metadata",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.UnpackPayload;
        public override Guid ComponentGuid => new Guid("6bebe05e-2605-4a12-9496-881d622fd731");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Json String", "Txt", "Serialized JSON String", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new PayloadParam(), "Payloads", "P", "Payload packets to be deserialize", GH_ParamAccess.list);
            pManager.AddParameter(new JsonDictParam(), "Metadata", "#", "Metadata that describe the payload",
                GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string dataStr = string.Empty;

            if (!DA.GetData(0, ref dataStr)) return;

            Payload deserializePayload = DeserializePayload(dataStr);

            // access the items and metadata
            JsonDict meta = deserializePayload.Meta;
            JsonDictGoo metaGoo = meta == null ? new JsonDictGoo() : new JsonDictGoo(meta);
            // convert to PayloadGoos
            List<PayloadGoo> items = deserializePayload.Items as List<PayloadGoo>;


            DA.SetDataList(0, items);
            DA.SetData(1, metaGoo);
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

        public Payload DeserializePayload(string json)
        {
            var payload = new Payload();

            try
            {
                JObject rootObject = JObject.Parse(json);
                var metaObj = rootObject["Meta"] as JObject;
                JToken itemsToken = rootObject["Items"];

                if (metaObj != null)
                {
                    payload.Meta = metaObj.ToObject<JsonDict>();
                }

                if (itemsToken != null && TryParseJson(itemsToken.ToString(), out JArray jArray))
                {
                    payload.Items = UnpackPayload(jArray);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            }

            return payload;
        }
    }
}