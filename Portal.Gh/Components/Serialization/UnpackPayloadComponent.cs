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

        public override GH_Exposure Exposure => GH_Exposure.septenary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.UnpackPayload;
        public override Guid ComponentGuid => new Guid("45c9911b-d669-4b3a-b35b-acf24c25aae2");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Json String", "Txt", "Serialized JSON String", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new JsonDictParam(), "Data", "Data", "The actual data of the payload",
                GH_ParamAccess.list);
            pManager.AddParameter(new JsonDictParam(), "Metadata", "Meta", "Metadata of each data entry",
                GH_ParamAccess.list);
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

            DA.SetDataList(0, payload.Item1);
            DA.SetDataList(1, payload.Item2);
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

        private (List<JsonDictGoo>, List<JsonDictGoo>) UnpackPayload(JArray jArray)
        {
            var dataList = new List<JsonDictGoo>();
            var metaList = new List<JsonDictGoo>();

            try
            {
                foreach (var item in jArray)
                {
                    var dataObj = item["Data"] as JObject;
                    var metaObj = item["Metadata"] as JObject;

                    if (dataObj != null)
                    {
                        var dataDict = dataObj.ToObject<JsonDict>();
                        dataList.Add(new JsonDictGoo(dataDict));
                    }

                    if (metaObj != null)
                    {
                        var metaDict = metaObj.ToObject<JsonDict>();
                        metaList.Add(new JsonDictGoo(metaDict));
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle parsing error
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error processing JSON:\n {ex.Message}");
            }

            return (dataList, metaList);
        }
    }
}