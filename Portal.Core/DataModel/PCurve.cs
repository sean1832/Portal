using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public abstract class PCurve : PEntity
    {
        public List<PVector3D> Points { get; protected set; }
        public CurveType CurveType { get; protected set; }
        protected PCurve(PType type) : base(type) { }
    }

    public class PNurbsCurve : PCurve {
        public bool IsPeriodic { get; }
        public int Degree { get; }

        public PNurbsCurve(List<PVector3D> points, bool isPeriodic, int degree) : base(PType.Curve)
        {
            CurveType = CurveType.Nurbs;
            Points = points;
            IsPeriodic = isPeriodic;
            Degree = degree;
        }
    }

    public class PLine: PCurve {

        public PLine(PVector3D start, PVector3D end): base(PType.Curve)
        {
            CurveType = CurveType.Line;
            Points[0] = start;
            Points[1] = end;
        }

        public PLine(List<PVector3D> points) : base(PType.Curve)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Count != 2) throw new ArgumentException("Line must have exactly 2 points");
            Points = points;
        }
    }

    public class PPolylineCurve : PCurve
    {
        public PPolylineCurve(List<PVector3D> points) : base(PType.Curve)
        {
            CurveType = CurveType.Polyline;
            Points = points;
        }
    }

    public class PCircle : PCurve
    {
        public PPlane Plane { get; set; }
        public double Radius { get; set; }

        public PCircle(PPlane plane, double radius) : base(PType.Curve)
        {
            CurveType = CurveType.Circle;
            Plane = plane;
            Radius = radius;
        }

        protected PCircle(PPlane plane, double radius, PType type)
            : base(type)
        {
            CurveType = CurveType.Circle;
            Plane = plane;
            Radius = radius;
        }
    }

    public class PArcCurve : PCircle
    {
        public double AngleRadiant { get; set; }

        public PArcCurve(PPlane plane, double radius, double angleRadiant) : base(plane, radius, PType.Curve)
        {
            CurveType = CurveType.Arc;
            AngleRadiant = angleRadiant;
        }
    }

    
}
