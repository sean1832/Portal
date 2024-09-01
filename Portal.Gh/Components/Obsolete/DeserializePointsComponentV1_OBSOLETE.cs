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
    public class DeserializePointsComponentV1_OBSOLETE : GH_Component
    {
        #region Metadata

        public DeserializePointsComponentV1_OBSOLETE()
            : base("Deserialize Points", "DSrPt",
                "Deserialize JSON string into points. This data can be read from communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { "desrpt" };
        protected override Bitmap Icon => Icons.DeserializePoint;
        public override Guid ComponentGuid => new Guid("dbcd87fc-6aad-409f-b89b-f0c56fd9a376");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Json data", "Json", "Json data to deserialize", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Deserialized points", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string data = string.Empty;

            if (!DA.GetData(0, ref data)) return;

            var points = DeserializePoints(data);

            DA.SetData(0, points);
        }

        private Point3d DeserializePoints(string data)
        {
            try
            {
                dynamic pPoint = JsonConvert.DeserializeObject<dynamic>(data);

                if (pPoint == null)
                {
                    throw new JsonSerializationException("Data is null. Check JSON.");
                }

                if (!PVector3D.Validate(pPoint))
                {
                    throw new JsonSerializationException("Deserialization resulted in invalid data. Check the JSON structure.");
                }

                Point3d point = new Point3d(pPoint.X, pPoint.Y, pPoint.Z);

                return point;
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
            return new Point3d();
        }
    }
}