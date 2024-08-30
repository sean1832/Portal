using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Newtonsoft.Json;
using Portal.Core.DataModel;
using Portal.Gh.Common;
using Portal.Gh.Params.Json;

namespace Portal.Gh.Components.Serialization
{
    public class SerializeMetadataComponent : GH_Component, IGH_VariableParameterComponent
    {
        // Constructor
        public SerializeMetadataComponent()
            : base("Serialize Metadata", "SrMeta",
                "Serialize payload metadata as JSON objects. This data should be pack as payload using `Pack Payload` before send over communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
            Instances.DocumentServer.DocumentRemoved += OnDocumentClose;
        }

        // Metadata
        public override GH_Exposure Exposure => GH_Exposure.septenary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("c9c8fc38-4ce3-4b71-af1d-8497c9763cd0");

        // IO
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            // Initially, no input parameters are added
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new JsonDictParam(), "Json", "Json", "output json object", GH_ParamAccess.item);
        }

        // Event handler for document close
        private void OnDocumentClose(GH_DocumentServer sender, GH_Document doc)
        {
        }

        // SolveInstance
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var valueOutput = new JsonDict();
            for (var i = 0; i < Params.Input.Count; i++)
            {
                var name = Params.Input[i].NickName;
                var access = Params.Input[i].Access;
                try
                {
                    switch (access)
                    {
                        case GH_ParamAccess.item:
                            dynamic dataValue = null;
                            DA.GetData(i, ref dataValue);
                            var rawValue = dataValue?.Value;
                            if (rawValue != null)
                            {
                                valueOutput[name] = rawValue;
                            }

                            break;
                        case GH_ParamAccess.list:
                            var dataValues = new List<dynamic>();
                            DA.GetDataList(i, dataValues);
                            if (dataValues.Any(v => v != null))
                            {
                                valueOutput[name] = dataValues.Select(v => v?.Value);
                            }

                            break;
                    }
                }
                catch (Exception e)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, e.Message);
                }
                if (valueOutput.Count > 0) DA.SetData("Json", new JsonDictGoo(valueOutput));
            }
        }


        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return side == GH_ParameterSide.Input;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            var param = new JsonDictGenericParam() { NickName = "-" };
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            for (var i = 0; i < Params.Input.Count; i++)
            {
                var param = Params.Input[i];
                if (param.NickName == "-")
                {
                    param.Name = $"Data {i + 1}";
                    param.NickName = $"d{i + 1}";
                }
                else
                {
                    param.Name = param.NickName;
                }
                param.Description = $"Input {i + 1}";
                param.Optional = true;
                param.MutableNickName = true;
            }
        }
    }
}
