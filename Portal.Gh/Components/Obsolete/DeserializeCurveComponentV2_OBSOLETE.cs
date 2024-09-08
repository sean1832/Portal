using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using Portal.Core.DataModel;
using Portal.Gh.Common;
using Newtonsoft.Json.Serialization;
using Portal.Gh.Components.Serialization.JsonSerializerSettings;
using Portal.Gh.Params.Json;
using Portal.Gh.Params.Payloads;

namespace Portal.Gh.Components.Obsolete
{
    public class DeserializeCurveComponentV2_OBSOLETE : GH_Component
    {
        #region Metadata

        public DeserializeCurveComponentV2_OBSOLETE()
            : base("Deserialize Curve", "DSrCrv",
                "Deserialize JSON string into curves. This data can be read from communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { "deserialize crv", "desrcrv" };
        protected override Bitmap Icon => Icons.DeserializeCurve;
        public override Guid ComponentGuid => new Guid("62cc044a-5d17-45a8-bdd9-96615fbd506c");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PayloadParam(), "Payload", "P", "Payload packet to be deserialize", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "crv", "Deserialized curve", GH_ParamAccess.item);
            pManager.AddParameter(new JsonDictParam(), "Metadata", "#", "Metadata that describe the geometry",
                GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PayloadGoo payloadGoo = new PayloadGoo();

            if (!DA.GetData(0, ref payloadGoo)) return;

            var curve = DeserializeCurve(payloadGoo.Value.Items.ToString());
            var meta = new JsonDictGoo(payloadGoo.Value.Meta);
            DA.SetData(0, curve);
            DA.SetData(1, meta);
        }


        private Curve DeserializeCurve(string jsonData)
        {
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.Converters.Add(new PCurveConverterSettings());

            PCurve pCurve = JsonConvert.DeserializeObject<PCurve>(jsonData, serializerSettings);

            switch (pCurve)
            {
                case PNurbsCurve nc:
                    return NurbsCurve.Create(
                        nc is { IsPeriodic: true },
                        nc.Degree,
                        nc.Points.Select(point => new Point3d(point.X, point.Y, point.Z))
                    );
                case PLine lc:
                    return new LineCurve(
                        new Point3d(lc.Points[0].X, lc.Points[0].Y, lc.Points[0].Z),
                        new Point3d(lc.Points[1].X, lc.Points[1].Y, lc.Points[1].Z)
                    );
                case PPolylineCurve pc:
                    return new PolylineCurve(pc.Points.Select(point => new Point3d(point.X, point.Y, point.Z)));
                case PArcCurve arc:
                    return ConstructCurve(arc);
                default:
                    throw new NotImplementedException($"Deserialization of {pCurve.GeoGeoType} is not implemented");
            }
        }


        private ArcCurve ConstructCurve(PArcCurve curve)
        {
            Plane plane = new Plane(
                new Point3d(curve.Plane.Origin.X, curve.Plane.Origin.Y, curve.Plane.Origin.Z),
                new Vector3d(curve.Plane.XAxis.X, curve.Plane.XAxis.Y, curve.Plane.XAxis.Z),
                new Vector3d(curve.Plane.YAxis.X, curve.Plane.YAxis.Y, curve.Plane.YAxis.Z)
            );
            return new ArcCurve(new Arc(plane, curve.Radius, curve.AngleRadiant));
        }
    }
}