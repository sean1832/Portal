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
using Newtonsoft.Json;
using Portal.Core.DataModel;
using Point = Rhino.Geometry.Point;

namespace Portal.Gh.Components.Serialization
{
    public class SerializePointComponent : GH_Component
    {
        #region Metadata

        public SerializePointComponent()
            : base("Serialize Points", "SrPt",
                "Serialize points into a JSON representation. This data can be send over communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("b29a2f0f-34a2-4207-a4ed-b597a803f7a3");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Points to serialize", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("JsonData", "Json", "Serialized curve as JSON", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> data = new List<Point3d>();

            if (!DA.GetDataList(0, data)) return;

            DA.SetData(0, SerializePoints(data));
        }

        private string SerializePoints(List<Point3d> points)
        {
            List<PVector3D> pPoints = new List<PVector3D>();
            foreach (var pt in points)
            {
                pPoints.Add(new PVector3D(pt.X, pt.Y, pt.Z));
            }
            return JsonConvert.SerializeObject(pPoints);
        }
    }
}