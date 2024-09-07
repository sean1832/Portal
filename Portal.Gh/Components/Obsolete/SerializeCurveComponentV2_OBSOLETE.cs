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
using Portal.Gh.Params.Json;
using Circle = Rhino.Geometry.Circle;
using System.Security.Cryptography;
using Portal.Gh.Params.Payloads;

namespace Portal.Gh.Components.Obsolete
{
    public class SerializeCurveComponentV2_OBSOLETE : GH_Component
    {
        #region Metadata

        public SerializeCurveComponentV2_OBSOLETE()
            : base("Serialize Curve", "SrCrv",
                "Serialize curves into a JSON representation. This data should be pack as payload using `Pack Payload` before send over communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { "serialize crv" };
        protected override Bitmap Icon => Icons.SerializeCurve;
        public override Guid ComponentGuid => new Guid("bf58221b-cf92-4bb3-a9c5-32070571bc4a");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "crv", "Curve to serialize", GH_ParamAccess.item);
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
            Curve curves = null;
            JsonDictGoo metaGoo = new JsonDictGoo();

            if (!DA.GetData(0, ref curves)) return;
            DA.GetData(1, ref metaGoo);

            DA.SetData(0, SerializeCurve(curves, metaGoo.Value));
        }

        private PayloadGoo SerializeCurve(Curve curve, JsonDict meta)
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
            string jsonString = JsonConvert.SerializeObject(pCurve);
            JsonDict dict = JsonConvert.DeserializeObject<JsonDict>(jsonString);
            return new PayloadGoo(new Payload(dict, meta));
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