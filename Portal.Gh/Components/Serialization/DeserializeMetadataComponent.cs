// reference JSwan by andrew heumann:
// https://github.com/andrewheumann/jSwan/blob/master/jSwan/Deserialize.cs
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Gh.Common;
using GH_IO.Serialization;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
using Portal.Gh.Params.Json;

namespace Portal.Gh.Components.Serialization
{
    public class DeserializeMetadataComponent : GH_Component, IGH_VariableParameterComponent
    {
        Dictionary<string, Type> uniqueChildProperties;
        public DeserializeMetadataComponent()
            : base("Deserialize Metadata", "DSrMeta",
                "Deserialize metadata into individual field",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.DeserializeMeta;
        public override Guid ComponentGuid => new Guid("473c030c-856b-4551-9ac6-65c9ccfbd3e6");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Json", "Json", "Json string to deconstruct", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var json = "";
            DA.GetData(0, ref json);


            if (DA.Iteration == 0)
            {
                var allData = Params.Input.OfType<Param_String>()
               .First()
               .VolatileData.AllData(true)
               .OfType<GH_String>()
               .Select(s => DeserializeToObject(s.Value));
                if (!allData.Any())
                {
                    return;
                }
                var children = allData.SelectMany(d => d?.Children() ?? new JEnumerable<JToken>()).ToList();

                var allProperties = children.OfType<JProperty>();

                uniqueChildProperties = new Dictionary<string, Type>();

                foreach (var property in allProperties)
                {
                    if (!uniqueChildProperties.ContainsKey(property.Name))
                    {
                        uniqueChildProperties.Add(property.Name, property.Value.GetType());
                    }
                }

                var names = allProperties.Select(c => c.Name).Distinct().ToList();
            }

            if (uniqueChildProperties.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No valid JSON properties found");
            }


            if (OutputMismatch()  && DA.Iteration == 0)
            {
                OnPingDocument().ScheduleSolution(5, d =>
                {
                    AutoCreateOutputs(false);
                });
            }
            else
            {
                var deserialized = DeserializeToObject(json);
                if (deserialized == null)
                {
                    return;
                }
                var children = deserialized.Children().ToList();

                for (var i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    if (child is JProperty property)
                    {

                        if (!Params.Output.Any(p => p.Name == property.Name))
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "There's a property here that doesn't match the outputs...");
                            continue;
                        }


                        if (property.Value is JArray array)
                        {
                            DA.SetDataList(property.Name, array.Select(t => t.ToSimpleValue()));
                        }
                        else
                        {
                            DA.SetData(property.Name, property.Value.ToSimpleValue());
                        }
                    }
                    else
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Found a child that's not a property. IDK what do with that yet");
                    }
                }
            }
        }

        private JObject DeserializeToObject(string json)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject(json);
                switch (obj)
                {
                    case JObject jobj:
                        return jobj;
                    case JArray jarr:
                    {
                        var newObj = new JObject { { "array", jarr } };
                        return newObj;
                    }
                    default:
                        return JsonConvert.DeserializeObject<JObject>(json);
                }
            }
            catch
            {
                return null;
            }
        }

        public override void ClearData()
        {
            base.ClearData();
            uniqueChildProperties?.Clear();
            if (Params == null || !Params.Any()) return;
            foreach (var ghParam in Params)
            {
                ghParam?.ClearData();
            }
        }

        private bool OutputMismatch()
        {
            var countMatch = uniqueChildProperties.Count() == Params.Output.Count;
            if (!countMatch) return true;

            foreach (var name in uniqueChildProperties)
            {
                if (Params.Output.Select(p => p.NickName).All(n => n != name.Key))
                {
                    return true;
                }
            }

            return false;
        }

        private void AutoCreateOutputs(bool recompute)
        {

            var tokenCount = uniqueChildProperties.Count();
            if (tokenCount == 0) return;

            if (OutputMismatch())
            {
                RecordUndoEvent("Output from Json");
                if (Params.Output.Count < tokenCount)
                {
                    while (Params.Output.Count < tokenCount)
                    {
                        var new_param = CreateParameter(GH_ParameterSide.Output, Params.Output.Count);
                        Params.RegisterOutputParam(new_param);
                    }
                }
                else if (Params.Output.Count > tokenCount)
                {
                    while (Params.Output.Count > tokenCount)
                    {
                        Params.UnregisterOutputParameter(Params.Output[Params.Output.Count - 1]);
                    }
                }
                Params.OnParametersChanged();
                VariableParameterMaintenance();
                ExpireSolution(recompute);
            }
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            return new Param_GenericObject();
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            var tokens = uniqueChildProperties;
            if (tokens == null) return;
            var names = tokens.Keys.ToList();
            for (var i = 0; i < Params.Output.Count; i++)
            {
                if (i > names.Count - 1) return;
                var name = names[i];
                var type = tokens[name];

                Params.Output[i].Name = $"{name}";
                Params.Output[i].NickName = $"{name}";
                Params.Output[i].Description = $"Data from property: {name}";
                Params.Output[i].MutableNickName = false;
                if (type.IsAssignableFrom(typeof(JArray)))
                {
                    Params.Output[i].Access = GH_ParamAccess.list;
                }
                else
                {
                    Params.Output[i].Access = GH_ParamAccess.item;

                }
            }
        }

        

        public void Dispose()
        {
            ClearData();
            foreach (var ghParam in Params)
            {
                ghParam.ClearData();
            }
        }
    }
}