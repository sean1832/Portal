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
using Rhino.DocObjects;

namespace Portal.Gh.Components.Utils
{
    public class GetCameraComponent : GH_Component
    {
        public GetCameraComponent()
            : base("Get Camera", "Camera",
                "Get Rhino viewport camera information. \nAttach a `trigger` component for dynamic update.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.septenary;
        public override IEnumerable<string> Keywords => new string[] { "getcamera" };
        protected override Bitmap Icon => Icons.GetCamera;
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

            RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
            if (doc == null) return;
            var viewport = doc.Views.ActiveView.ActiveViewport;

            // Get camera angles
            // see https://discourse.mcneel.com/t/getting-field-of-view-of-a-viewport-inside-of-rhinocommon/88724/3
            var vpInfo = new ViewportInfo(viewport);
            if (!vpInfo.GetCameraAngles(
                    out double halfDiagonalAngleRadians,
                    out double halfVerticalAngleRadians,
                    out double halfHorizontalAngleRadians)
               )
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Failed to get camera angles.");
                return;
            }

            PCamera cam = new PCamera(
                PTypeConverter.ToPVector3D(viewport.CameraLocation),
                PTypeConverter.ToPVector3D(viewport.CameraDirection),
                (float)viewport.Camera35mmLensLength,
                sensorWidth,
                new PVector2Di(viewport.Size)
            );
            double verticalFov = PCamera.CalculateFov(halfVerticalAngleRadians * 2.0);
            double horizontalFov = PCamera.CalculateFov(halfHorizontalAngleRadians * 2.0);
            cam.SetVerticalFov((float)verticalFov);
            cam.SetHorizontalFov((float)horizontalFov);
            
            string camStr = JsonConvert.SerializeObject(cam);
            JsonDict camDict = JsonConvert.DeserializeObject<JsonDict>(camStr);

            DA.SetData(0, new JsonDictGoo(camDict));
        }
    }
}