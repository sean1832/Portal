using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel.Parameters;
using Newtonsoft.Json;
using Portal.Core.DataModel;
using Portal.Gh.Common;
using Portal.Gh.Params.Json;
using Portal.Gh.Params.Payloads;
using Rhino.DocObjects;
using Point = Rhino.Geometry.Point;
using Grasshopper.Kernel.Types;
using Eto.Drawing;
using Rhino;
using Bitmap = System.Drawing.Bitmap;

namespace Portal.Gh.Components.Serialization
{
    public class SerializeGeometryComponent : GH_Component
    {
        public SerializeGeometryComponent()
            : base("Serialize Geometry", "SrGeo",
                "Serialize geometry into structured JSON packet",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.SerializeGeo;
        public override Guid ComponentGuid => new Guid("9c14644b-81f5-4d7e-acdf-e2dda6fa5120");

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "G", "Geometry to serialize", GH_ParamAccess.item);
            pManager.AddParameter(new JsonDictParam(), "Metadata", "#", "(Optional) Metadata that describe the geometry",
                GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new PayloadParam(), "Payload", "P", "Structured packet payload", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_GeometricGoo inputGoo = null;
            JsonDictGoo metaGoo = new JsonDictGoo();

            if (!DA.GetData(0, ref inputGoo)) return;
            DA.GetData(1, ref metaGoo);

            if (inputGoo is GH_Point pointGoo)
            {
                if (!pointGoo.CastTo(out Point3d pointValue))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unable to extract Point3d from GH_Point.");
                    return;
                }
                DA.SetData(0, new PayloadGoo(SerializeGeometry(pointValue, metaGoo.Value)));
                return;
            }
            if (!inputGoo.CastTo(out GeometryBase geometry))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Unable to extract geometry: {inputGoo.TypeName}. This type may not be supported.");
                return;
            }

            DA.SetData(0, new PayloadGoo(SerializeGeometry(geometry, metaGoo.Value)));
        }

        private Payload SerializeGeometry(GeometryBase geo, JsonDict meta)
        {
            string dataItem;
            switch (geo.ObjectType)
            {
                case ObjectType.Mesh:
                    dataItem = JsonConvert.SerializeObject(SerializeMesh((Mesh)geo));
                    break;
                case ObjectType.Curve:
                    dataItem = JsonConvert.SerializeObject(SerializeCurve((Curve)geo));
                    break;
                case ObjectType.Light:
                    dataItem = JsonConvert.SerializeObject(SerializeLight((Light)geo));
                    break;
                default:
                    throw new NotImplementedException($"Serialization for this geometry type: '{geo.ObjectType}' is not implemented.");
            }

            JsonDict dict = JsonConvert.DeserializeObject<JsonDict>(dataItem);
            return new Payload(dict, meta);
        }

        private Payload SerializeGeometry(Point3d geo, JsonDict meta)
        {
            string jsonString = JsonConvert.SerializeObject(new PVector3D(geo.X, geo.Y, geo.Z));
            JsonDict dict = JsonConvert.DeserializeObject<JsonDict>(jsonString);
            return new Payload(dict, meta);
        }

        private PMesh SerializeMesh(Mesh mesh)
        {
            var vertices = mesh.Vertices.Select(vertex => new PVector3Df(vertex.X, vertex.Y, vertex.Z)).ToList();
            var faces = mesh.Faces.Select(face => new[] { face.A, face.B, face.C, face.D }).ToList();
            var vertexColors = mesh.VertexColors.Select(color =>
            {
                var pColor = new PColor(color.R, color.G, color.B, color.A);
                return pColor.ToHex();
            }).ToList();
            List<PVector2Df> uvs = mesh.TextureCoordinates.Select(uv => new PVector2Df(uv.X, uv.Y)).ToList();

            return new PMesh(vertices, faces, vertexColors, uvs);
        }

        private PLight SerializeLight(Light light)//add 0925
        {
            return light switch
            {
                var l when l.IsPointLight => new PPointLight
                (
                    "POINT",
                    l.Diffuse,
                    l.AttenuationType,
                    new PVector3D(l.Location.X, l.Location.Y, l.Location.Z),
                    l.Intensity
                ),
                var l when l.IsRectangularLight => new PRectangularLight
                (
                    "AREA",
                    l.Diffuse,
                    l.AttenuationType,
                    new PVector3D(l.Location.X, l.Location.Y, l.Location.Z),
                    new PVector3D(l.Direction.X, l.Direction.Y, l.Direction.Z),
                    l.Intensity,
                    new PVector3D(l.Length.X, l.Length.Y, l.Length.Z),
                    new PVector3D(l.Width.X, l.Width.Y, l.Width.Z)
                ),
                var l when l.IsSpotLight => new PSpotLight
                (
                    "SPOT",
                    l.Diffuse,
                    l.AttenuationType,
                    new PVector3D(l.Location.X, l.Location.Y, l.Location.Z),
                    new PVector3D(l.Direction.X, l.Direction.Y, l.Direction.Z),
                    l.Intensity, 
                    l.SpotAngleRadians,
                    l.HotSpot, 
                    l.ShadowIntensity
                ),
                var l when l.IsSunLight => new PSunLight
                (
                    "SUN",
                    l.Diffuse,
                    l.AttenuationType,
                    new PVector3D(l.Location.X, l.Location.Y, l.Location.Z),
                    l.Intensity
                ),
            };
        }
        private PCurve SerializeCurve(Curve curve)
        {
            return curve switch
            {
                NurbsCurve nc => new PNurbsCurve(ConvertPVector3(nc), nc.IsPeriodic, nc.Degree),
                PolylineCurve pc => new PPolylineCurve(ConvertPVector3(pc)),
                LineCurve lc => new PLine(ConvertPVector3(lc)),
                ArcCurve arc => ConvertPArcCurve(arc),
                _ => new PNurbsCurve(ConvertPVector3(curve.ToNurbsCurve()), curve.ToNurbsCurve().IsPeriodic, curve.ToNurbsCurve().Degree),
            };
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
            return new List<PVector3D>
            {
                new PVector3D(lineCurve.Line.From.X, lineCurve.Line.From.Y, lineCurve.Line.From.Z),
                new PVector3D(lineCurve.Line.To.X, lineCurve.Line.To.Y, lineCurve.Line.To.Z)
            };
        }

        private PVector3D SerializePoint(Point3d pt)
        {
            return new PVector3D(pt.X, pt.Y, pt.Z);
        }

        private PVector3Df SerializePoint(Point3f pt)
        {
            return new PVector3Df(pt.X, pt.Y, pt.Z);
        }
    }
}