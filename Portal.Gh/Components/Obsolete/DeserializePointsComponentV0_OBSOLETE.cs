using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using Portal.Core.DataModel;
using Portal.Gh.Common;

namespace Portal.Gh.Components.Obsolete
{
    public class DeserializePointsComponentV0_OBSOLETE : GH_Component
    {
        #region Metadata

        public DeserializePointsComponentV0_OBSOLETE()
            : base("Deserialize Points", "DSrPt",
                "Deserialize JSON string into points. This data can be read from communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { "desrpt" };
        protected override Bitmap Icon => Icons.DeserializePoint;
        public override Guid ComponentGuid => new Guid("59607625-a1e4-4198-bae0-857bd4458258");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Json data", "Json", "Json data to deserialize", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Deserialized points", GH_ParamAccess.list);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string data = string.Empty;

            if (!DA.GetData(0, ref data)) return;

            var points = DeserializePoints(data);

            DA.SetDataList(0, points);
        }

        private List<Point3d> DeserializePoints(string data)
        {
            try
            {
                List<dynamic> pPoints = JsonConvert.DeserializeObject<List<dynamic>>(data);
                List<Point3d> points = new List<Point3d>();

                if (pPoints == null)
                {
                    throw new JsonSerializationException("Data is null. Check JSON.");
                }

                foreach (var pPt in pPoints)
                {
                    if (!PVector3D.Validate(pPt))
                    {
                        throw new JsonSerializationException("Deserialization resulted in invalid data. Check the JSON structure.");
                    }
                    points.Add(new Point3d(pPt.X, pPt.Y, pPt.Z));
                }

                return points;
            }
            catch (JsonSerializationException e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error deserializing JSON data: {e.Message}");
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"An error occurred: {e.Message}");
            }

            // return empty list if deserialization fails
            return new List<Point3d>();
        }
    }
}