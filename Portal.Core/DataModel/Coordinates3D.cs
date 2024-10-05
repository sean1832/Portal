using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public abstract class Coordinates3D<T>
    {
        public T X { get; set; }
        public T Y { get; set; }
        public T Z { get; set; }

        protected Coordinates3D(T x, T y, T z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class PVector3D : Coordinates3D<double>
    {
        public PVector3D(double x, double y, double z) : base(x, y, z)
        {
        }

        public static bool Validate(dynamic obj)
        {
            try
            {
                // Check for existence and type of X, Y, Z properties
                return obj.X != null && obj.Y != null && obj.Z != null &&
                       double.TryParse(obj.X.ToString(), out double _) &&
                       double.TryParse(obj.Y.ToString(), out double _) &&
                       double.TryParse(obj.Z.ToString(), out double _);
            }
            catch
            {
                return false;
            }
        }
        public PVector3D Normalize()
        {
            var magnitude = Math.Sqrt(X * X + Y * Y + Z * Z);
            return new PVector3D(X / magnitude, Y / magnitude, Z / magnitude);
        }

        public bool IsNormalized(double tolerance = 0.0001)
        {
            return Math.Abs(Math.Sqrt(X * X + Y * Y + Z * Z) - 1) < tolerance;
        }

        public static PVector3D CrossProduct(PVector3D a, PVector3D b)
        {
            return new PVector3D(a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        public static double DotProduct(PVector3D a, PVector3D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static double AngleBetween(PVector3D a, PVector3D b)
        {
            return Math.Acos(DotProduct(a, b) / (a.Magnitude() * b.Magnitude()));
        }

        public double Magnitude()
        {
            // Pythagorean theorem
            // a^2 + b^2 = c^2
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        // operators for vector math
        public static PVector3D operator +(PVector3D a, PVector3D b) => new PVector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static PVector3D operator -(PVector3D a, PVector3D b) => new PVector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static PVector3D operator *(PVector3D a, double b) => new PVector3D(a.X * b, a.Y * b, a.Z * b);
        public static PVector3D operator /(PVector3D a, double b) => new PVector3D(a.X / b, a.Y / b, a.Z / b);
    }

    public class PVector3Df : Coordinates3D<float>
    {
        public PVector3Df(float x, float y, float z) : base(x, y, z)
        {
        }

        public static bool Validate(dynamic obj)
        {
            try
            {
                // Check for existence and type of X, Y, Z properties
                return obj.X != null && obj.Y != null && obj.Z != null &&
                       float.TryParse(obj.X.ToString(), out float _) &&
                       float.TryParse(obj.Y.ToString(), out float _) &&
                       float.TryParse(obj.Z.ToString(), out float _);
            }
            catch
            {
                return false;
            }
        }

        public PVector3Df Normalize()
        {
            var magnitude = (float)Math.Sqrt(X * X + Y * Y + Z * Z);
            return new PVector3Df(X / magnitude, Y / magnitude, Z / magnitude);
        }

        public bool IsNormalized(float tolerance = 0.0001f)
        {
            return Math.Abs(Math.Sqrt(X * X + Y * Y + Z * Z) - 1) < tolerance;
        }

        public static PVector3Df CrossProduct(PVector3Df a, PVector3Df b)
        {
            return new PVector3Df(a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        }

        public static double DotProduct(PVector3Df a, PVector3Df b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static double AngleBetween(PVector3Df a, PVector3Df b)
        {
            return Math.Acos(DotProduct(a, b) / (a.Magnitude() * b.Magnitude()));
        }

        public double Magnitude()
        {
            // Pythagorean theorem
            // a^2 + b^2 = c^2
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        // operators for vector math
        public static PVector3Df operator +(PVector3Df a, PVector3Df b) => new PVector3Df(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static PVector3Df operator -(PVector3Df a, PVector3Df b) => new PVector3Df(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static PVector3Df operator *(PVector3Df a, double b) => new PVector3Df(a.X * (float)b, a.Y * (float)b, a.Z * (float)b);
        public static PVector3Df operator /(PVector3Df a, double b) => new PVector3Df(a.X / (float)b, a.Y / (float)b, a.Z / (float)b);
    }

    
}