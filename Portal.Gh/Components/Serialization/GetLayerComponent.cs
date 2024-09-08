using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Gh.Common;
using Portal.Gh.Params.Json;
using Grasshopper.Kernel.Types;
using Portal.Core.DataModel;
using Rhino;

namespace Portal.Gh.Components.Serialization
{
    public class GetLayerComponent : GH_Component
    {
        public GetLayerComponent()
            : base("Get Layer", "Layer",
                "Get layer information of a Rhino object. \nAttach a `trigger` component for dynamic update.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.septenary;
        public override IEnumerable<string> Keywords => new string[] { "getlayer" };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("cf565aba-47bd-40ba-9f8d-256664bcf8ce");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "G", "Geometry to get layer from", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new JsonDictParam(), "Layers", "L", "Layer information in JSON format", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_GeometricGoo inputGoo = null;

            if (!DA.GetData(0, ref inputGoo)) return;

            if (inputGoo.ReferenceID == Guid.Empty)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Object is not referenced.");
                return;
            }

            var doc = RhinoDoc.ActiveDoc;
            var obj = doc.Objects.Find(inputGoo.ReferenceID);

            if (obj == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Object not found.");
                return;
            }

            var layer = doc.Layers.FindIndex(obj.Attributes.LayerIndex);
            JsonDict layerDict = new JsonDict
            {
                { "Id", layer.Id},
                { "FullPath", layer.FullPath},
                { "Color", new PColor(layer.Color).ToHex() },
                { "IsVisible", layer.IsVisible },
                { "IsLocked", layer.IsLocked }
            };

            DA.SetData(0, new JsonDictGoo(layerDict));
        }
    }
}