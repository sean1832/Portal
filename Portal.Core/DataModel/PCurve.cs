using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public abstract class PCurve
    {
        public string Type { get; protected set; }
        public List<PVector3D> Points { get; protected set; }

        protected PCurve() {}

        protected PCurve(string type, List<PVector3D> points)
        {
            Type = type;
            Points = points;
        }

        protected PCurve(List<PVector3D> points)
        {
            Type = nameof(PCurve);
            Points = points;
        }
    }

    public class PNurbsCurve : PCurve {
        public bool IsPeriodic { get; }
        public int Degree { get; }

        public PNurbsCurve(List<PVector3D> points, bool isPeriodic, int degree)
        {
            Type = nameof(PNurbsCurve);
            Points = points;
            IsPeriodic = isPeriodic;
            Degree = degree;
        }
    }

    public class PLine: PCurve {

        public PLine(PVector3D start, PVector3D end)
        {
            Type = nameof(PLine);
            Points[0] = start;
            Points[1] = end;
        }

        public PLine(List<PVector3D> points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Count != 2) throw new ArgumentException("Line must have exactly 2 points");

            Type = nameof(PLine);
            Points = points;
        }
    }

    public class PPolylineCurve : PCurve
    {
        public PPolylineCurve(List<PVector3D> points)
        {
            Type = nameof(PPolylineCurve);
            Points = points;
        }
    }

    public class PCircle : PCurve
    {
        public PPlane Plane { get; set; }
        public double Radius { get; set; }

        public PCircle(PPlane plane, double radius)
        {
            Type = nameof(PCircle);
            Plane = plane;
            Radius = radius;
        }
    }

    public class PArcCurve : PCircle
    {
        public double AngleRadiant { get; set; }

        public PArcCurve(PPlane plane, double radius, double angleRadiant) : base(plane, radius)
        {
            Type = nameof(PArcCurve);
            AngleRadiant = angleRadiant;
        }
    }

    
}
