using Grasshopper;
using Grasshopper.Kernel;
using Portal.Gh.Common;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using GH_IO.Serialization;
using Newtonsoft.Json;
using Portal.Core.DataModel;
using Portal.Gh.Params.Json;
using Point = Rhino.Geometry.Point;
using Rhino.Render.DataSources;

namespace Portal.Gh.Components.Obsolete
{
    public class SerializePointComponentV1_OBSOLETE : GH_Component
    {
        #region Metadata

        public SerializePointComponentV1_OBSOLETE()
            : base("Serialize Points", "SrPt",
                "Serialize points into a JSON representation. This data should be pack as payload using `Pack Payload` before send over communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.SerializePoint;
        public override Guid ComponentGuid => new Guid("cacea36d-2987-4b63-8975-8d0f886e1454");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Points to serialize", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new JsonDictParam(), "Json", "Json", "Serialized point as JSON object", GH_ParamAccess.item);
        }

        #endregion


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d data = new Point3d();

            if (!DA.GetData(0, ref data)) return;

            DA.SetData(0, SerializePoint(data));
        }
        private JsonDictGoo SerializePoint(Point3d point)
        {
            string jsonString = JsonConvert.SerializeObject(new PVector3D(point.X, point.Y, point.Z));
            JsonDict dict = JsonConvert.DeserializeObject<JsonDict>(jsonString);
            return new JsonDictGoo(dict);
        }
    }
}