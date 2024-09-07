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

        protected PCurve(string type) : base(type) { }
    }

    public class PNurbsCurve : PCurve {
        public bool IsPeriodic { get; }
        public int Degree { get; }

        public PNurbsCurve(List<PVector3D> points, bool isPeriodic, int degree) : base(nameof(PNurbsCurve))
        {
            Points = points;
            IsPeriodic = isPeriodic;
            Degree = degree;
        }
    }

    public class PLine: PCurve {

        public PLine(PVector3D start, PVector3D end): base(nameof(PLine))
        {
            Points[0] = start;
            Points[1] = end;
        }

        public PLine(List<PVector3D> points) : base(nameof(PLine))
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Count != 2) throw new ArgumentException("Line must have exactly 2 points");
            Points = points;
        }
    }

    public class PPolylineCurve : PCurve
    {
        public PPolylineCurve(List<PVector3D> points) : base(nameof(PPolylineCurve))
        {
            Points = points;
        }
    }

    public class PCircle : PCurve
    {
        public PPlane Plane { get; set; }
        public double Radius { get; set; }

        public PCircle(PPlane plane, double radius) : base(nameof(PCircle))
        {
            Plane = plane;
            Radius = radius;
        }

        protected PCircle(PPlane plane, double radius, string type)
            : base(type)
        {
            Plane = plane;
            Radius = radius;
        }
    }

    public class PArcCurve : PCircle
    {
        public double AngleRadiant { get; set; }

        public PArcCurve(PPlane plane, double radius, double angleRadiant) : base(plane, radius, nameof(PArcCurve))
        {
            AngleRadiant = angleRadiant;
        }
    }

    
}
