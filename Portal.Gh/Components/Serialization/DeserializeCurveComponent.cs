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

namespace Portal.Gh.Components.Serialization
{
    public class DeserializeCurveComponent : GH_Component
    {
        #region Metadata

        public DeserializeCurveComponent()
            : base("Deserialize Curve", "DSrlzCrv",
                "Deserialize JSON string into curves. This data can be read from communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "deserialize crv" };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("4d71d088-84fe-48fe-9a42-8bd33c35f5b9");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Json data", "Json", "Json data to deserialize", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "crv", "Deserialized curves", GH_ParamAccess.list);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string jsonData = string.Empty;

            if (!DA.GetData(0, ref jsonData)) return;

            DA.SetDataList(0, DeserializeCurve(jsonData));
        }

        private List<Curve> DeserializeCurve(string jsonData)
        {
            List<Curve> curves = new List<Curve>();
            var serializerSettings = new Newtonsoft.Json.JsonSerializerSettings();
            serializerSettings.Converters.Add(new PCurveConverterSettings());
            
            List<PCurve> pCurves = JsonConvert.DeserializeObject<List<PCurve>>(jsonData, serializerSettings);

            foreach (var pCurve in pCurves)
            {
                switch (pCurve)
                {
                    case PNurbsCurve nc:
                        curves.Add(NurbsCurve.Create(
                            nc is { IsPeriodic: true },
                            nc.Degree,
                            nc.Points.Select(point => new Point3d(point.X, point.Y, point.Z))
                        ));
                        break;
                    case PLine lc:
                        curves.Add(new LineCurve(
                            new Point3d(lc.Points[0].X, lc.Points[0].Y, lc.Points[0].Z),
                            new Point3d(lc.Points[1].X, lc.Points[1].Y, lc.Points[1].Z)
                        ));
                        break;
                    case PPolylineCurve pc:
                        curves.Add(new PolylineCurve(pc.Points.Select(point => new Point3d(point.X, point.Y, point.Z))));
                        break;
                    case PArcCurve arc:
                        curves.Add(ConstructCurve(arc));
                        break;
                    default:
                        throw new NotImplementedException($"Deserialization of {pCurve.Type} is not implemented");
                }
            }
            return curves;
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