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
using Circle = Rhino.Geometry.Circle;

namespace Portal.Gh.Components.Serialization
{
    public class SerializeCurveComponent : GH_Component
    {
        #region Metadata

        public SerializeCurveComponent()
            : base("Serialize Curve", "SrCrv",
                "Serialize curves into a JSON representation. This data can be send over communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override IEnumerable<string> Keywords => new string[] { "serialize crv" };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("a82975a6-9d14-4fa0-90c2-57fd36b49ab6");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "crv", "Curve to serialize", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("JsonData", "Json", "Serialized curve as JSON", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> curves = new List<Curve>();

            if (!DA.GetDataList(0, curves)) return;

            DA.SetData(0, SerializeCurve(curves));
        }

        private string SerializeCurve(List<Curve> curves)
        {
            List<PCurve> pCurves = new List<PCurve>();
            foreach (var curve in curves)
            {
                PCurve pCurve;
                switch (curve)
                {
                    case NurbsCurve nc:
                        pCurve = new PNurbsCurve(ConvertPVector3(nc), nc.IsPeriodic, nc.Degree);
                        break;
                    case PolylineCurve pc:
                        pCurve = new PPolylineCurve(ConvertPVector3(pc));
                        break;
                    case LineCurve lc:
                        pCurve = new PLine(ConvertPVector3(lc));
                        break;
                    case ArcCurve arc:
                        pCurve = ConvertPArcCurve(arc);
                        break;
                    default:
                        // default to nurbs curve
                        NurbsCurve defaultNc = curve.ToNurbsCurve();
                        pCurve = new PNurbsCurve(ConvertPVector3(defaultNc), defaultNc.IsPeriodic, defaultNc.Degree);
                        break;
                }
                pCurves.Add(pCurve);
            }
            return JsonConvert.SerializeObject(pCurves);
        }

        private PArcCurve ConvertPArcCurve(ArcCurve arcCurve)
        {
            PVector3D origin = new PVector3D(arcCurve.Arc.Plane.Origin.X, arcCurve.Arc.Plane.Origin.Y, arcCurve.Arc.Plane.Origin.Z);
            PVector3D xDirection = new PVector3D(arcCurve.Arc.Plane.XAxis.X, arcCurve.Arc.Plane.XAxis.Y, arcCurve.Arc.Plane.XAxis.Z);
            PVector3D yDirection = new PVector3D(arcCurve.Arc.Plane.YAxis.X, arcCurve.Arc.Plane.YAxis.Y, arcCurve.Arc.Plane.YAxis.Z);
            return new PArcCurve(
                new PPlane(origin, xDirection, yDirection),
                arcCurve.Radius,
                arcCurve.AngleRadians
                );
        }

        private List<PVector3D> ConvertPVector3(NurbsCurve nurbsCurve)
        {
            return nurbsCurve.Points.Select(point => new PVector3D(point.X, point.Y, point.Z)).ToList();
        }

        private List<PVector3D> ConvertPVector3(PolylineCurve polylineCurve)
        {
            List<PVector3D> points = new List<PVector3D>();
            if (polylineCurve.TryGetPolyline(out var polyline))
            {
                points.AddRange(polyline.Select(pt => new PVector3D(pt.X, pt.Y, pt.Z)));
            }
            return points;
        }

        private List<PVector3D> ConvertPVector3(LineCurve lineCurve)
        {
            List<PVector3D> points = new List<PVector3D>();
            points.Add(new PVector3D(lineCurve.Line.From.X, lineCurve.Line.From.Y, lineCurve.Line.From.Z));
            points.Add(new PVector3D(lineCurve.Line.To.X, lineCurve.Line.To.Y, lineCurve.Line.To.Z));
            return points;
        }
    }
}