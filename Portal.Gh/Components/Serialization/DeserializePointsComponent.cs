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
using Portal.Gh.Params.Json;
using Portal.Gh.Params.Payloads;

namespace Portal.Gh.Components.Serialization
{
    public class DeserializePointsComponent : GH_Component
    {
        #region Metadata

        public DeserializePointsComponent()
            : base("Deserialize Points", "DSrPt",
                "Deserialize JSON string into points. This data can be read from communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "desrpt" };
        protected override Bitmap Icon => Icons.DeserializePoint;
        public override Guid ComponentGuid => new Guid("4bdba56b-b589-4326-9683-6cbab0bddcc5");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PayloadParam(), "Payload", "P", "Payload packet to be deserialize", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Pt", "Deserialized point", GH_ParamAccess.item);
            pManager.AddParameter(new JsonDictParam(), "Metadata", "#", "Metadata that describe the geometry",
                GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PayloadGoo payloadGoo = new PayloadGoo();

            if (!DA.GetData(0, ref payloadGoo)) return;

            var point = DeserializePoint(payloadGoo.Value.Items.ToString());
            var meta = new JsonDictGoo(payloadGoo.Value.Meta);
            
            DA.SetData(0, point);
            DA.SetData(1, meta);
        }

        private Point3d? DeserializePoint(string data)
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
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Error deserializing JSON data: {e.Message}");
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"An error occurred: {e.Message}");
            }

            // return empty point if deserialization fails
            return null;
        }

    }
}