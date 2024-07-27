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
    }
}