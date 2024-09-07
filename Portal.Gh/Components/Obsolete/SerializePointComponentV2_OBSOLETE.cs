using Grasshopper.Kernel;
using Portal.Gh.Common;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using Portal.Core.DataModel;
using Portal.Gh.Params.Json;
using Portal.Gh.Params.Payloads;

namespace Portal.Gh.Components.Obsolete
{
    public class SerializePointComponentV2_OBSOLETE : GH_Component
    {
        #region Metadata

        public SerializePointComponentV2_OBSOLETE()
            : base("Serialize Points", "SrPt",
                "Serialize points into a JSON representation. This data should be pack as payload using `Pack Payload` before send over communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.SerializePoint;
        public override Guid ComponentGuid => new Guid("3c9bdd0a-ae6b-49ed-b95d-e1f98c3e5eeb");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Points to serialize", GH_ParamAccess.item);
            pManager.AddParameter(new JsonDictParam(), "Metadata", "#", "(Optional) Metadata that describe the geometry",
                GH_ParamAccess.item);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new PayloadParam(), "Payload", "P", "Structured packet payload", GH_ParamAccess.item);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d data = new Point3d();
            JsonDictGoo metaGoo = new JsonDictGoo();

            if (!DA.GetData(0, ref data)) return;
            DA.GetData(1, ref metaGoo);

            DA.SetData(0, SerializePoint(data, metaGoo.Value));
        }
        private PayloadGoo SerializePoint(Point3d point, JsonDict meta)
        {
            string jsonString = JsonConvert.SerializeObject(new PVector3D(point.X, point.Y, point.Z));
            JsonDict dict = JsonConvert.DeserializeObject<JsonDict>(jsonString);

            return new PayloadGoo(new Payload(dict, meta));
        }
    }
}