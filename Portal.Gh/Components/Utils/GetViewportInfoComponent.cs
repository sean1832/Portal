using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using Portal.Core.DataModel;
using Portal.Gh.Common;
using Portal.Gh.Params.Json;
using Rhino;

namespace Portal.Gh.Components.Utils
{
    public class GetViewportInfoComponent : GH_Component
    {
        public GetViewportInfoComponent()
            : base("Get Viewport Info", "ViewInfo",
                "Description",
                Config.Category, Config.SubCat.Utils)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("595ccdf0-2918-41fd-a0d9-00a9ab921d60");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new JsonDictParam(), "Camera", "C", "Camera information", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            float sensorWidth = 36.0f; // Standard width of a 35mm film camera sensor in mm
            int interval = 10; // Interval in ms


            RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
            if (doc == null) return;
            var viewport = doc.Views.ActiveView.ActiveViewport;
            PCamera cam = new PCamera(
                PTypeConverter.ToPVector3D(viewport.CameraLocation),
                PTypeConverter.ToPVector3D(viewport.CameraDirection),
                viewport.Camera35mmLensLength,
                sensorWidth,
                new PVector2Di(viewport.Size)
            );

            string camStr = JsonConvert.SerializeObject(cam);
            JsonDict camDict = JsonConvert.DeserializeObject<JsonDict>(camStr);

            DA.SetData(0, new JsonDictGoo(camDict));
        }
    }
}