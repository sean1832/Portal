using Grasshopper;
using Grasshopper.Kernel;
using Portal.Gh.Common;
using Portal.Gh.Params.Payloads;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Rhino.DocObjects;
using Portal.Core.DataModel;
using Newtonsoft.Json;
using Portal.Gh.Params.Json;
using Rhino.FileIO;

namespace Portal.Gh.Components.Serialization
{
    public class GetLightComponent : GH_Component
    {
        public GetLightComponent()
            : base("Get Light", "SrLight",
                "Serialize rhino's light object into JSON object",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.septenary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.GetLight;
        public override Guid ComponentGuid => new Guid("2984c41c-fdb4-4679-8021-d17c7d6d4ea4");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Light GUID", "GUID", "The GUID of the Rhino light object", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new JsonDictParam(), "Light", "L", "Serialized Light object", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Retrieve the GUID input
            string lightGuidString = string.Empty;
            if (!DA.GetData(0, ref lightGuidString)) return;

            // Parse GUID
            if (!Guid.TryParse(lightGuidString, out var lightGuid))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid GUID");
                return;
            }

            // Retrieve the light object from the Rhino document
            RhinoObject obj = Rhino.RhinoDoc.ActiveDoc.Objects.FindId(lightGuid);
            if (obj == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Object not found");
                return;
            }

            // Check if the object is a light
            if (obj.ObjectType != ObjectType.Light)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Object {obj.ObjectType.ToString()} is not a light.");
                return;
            }

            // Retrieve the light object
            LightObject light = obj as LightObject;
            if (light == null) {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Light object is null");
                return;
            }

            // Serialize the light object
            var dataItem = JsonConvert.SerializeObject(SerializeLight(light.LightGeometry));
            JsonDict dict = JsonConvert.DeserializeObject<JsonDict>(dataItem);

            DA.SetData(0, new JsonDictGoo(dict));
        }

        private PLight SerializeLight(Light light)//add 0925
        {
            switch (light)
            {
                case var l when l.IsPointLight:
                    return new PPointLight(l.Name, l.Diffuse, (PAttenuation)l.AttenuationType, 
                        PTypeConverter.ToPVector3D(l.AttenuationVector),
                        PTypeConverter.ToPVector3D(l.Location), l.Intensity, l.ShadowIntensity);
                case var l when l.IsRectangularLight:
                    return new PRectangularLight(l.Name, l.Diffuse, (PAttenuation)l.AttenuationType,
                        PTypeConverter.ToPVector3D(l.AttenuationVector),
                        PTypeConverter.ToPVector3D(l.Location),
                        PTypeConverter.ToPVector3D(l.Direction), l.Intensity, l.ShadowIntensity,
                        PTypeConverter.ToPVector3D(l.Length),
                        PTypeConverter.ToPVector3D(l.Width));
                case var l when l.IsSpotLight:
                    l.GetSpotLightRadii(out double inner, out double outer);
                    return new PSpotLight(l.Name, l.Diffuse, (PAttenuation)l.AttenuationType,
                        PTypeConverter.ToPVector3D(l.AttenuationVector),
                        PTypeConverter.ToPVector3D(l.Location),
                        PTypeConverter.ToPVector3D(l.Direction), l.Intensity,
                        l.SpotAngleRadians,
                        l.HotSpot, l.ShadowIntensity, new PSpotRadii(inner, outer));
                case var l when l.IsSunLight || l.IsDirectionalLight:
                    return new PDirectionalLight(l.Name, l.Diffuse, (PAttenuation)l.AttenuationType,
                        PTypeConverter.ToPVector3D(l.AttenuationVector),
                        PTypeConverter.ToPVector3D(l.Location),
                        PTypeConverter.ToPVector3D(l.Direction),
                        l.Intensity, l.ShadowIntensity);
                case var l when l.IsLinearLight:
                    return new PLinearLight(l.Name, l.Diffuse, (PAttenuation)l.AttenuationType, 
                        PTypeConverter.ToPVector3D(l.AttenuationVector),
                        PTypeConverter.ToPVector3D(l.Location),
                        PTypeConverter.ToPVector3D(l.Direction),
                        l.Intensity, l.ShadowIntensity);
                default:
                    throw new ArgumentOutOfRangeException(nameof(light));
            }
        }
    }
}