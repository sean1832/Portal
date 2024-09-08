using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Portal.Core.DataModel;
using Rhino.Geometry;

namespace Portal.Gh.Common
{
    internal static class PTypeConverter
    {
        public static PVector3D ToPVector3D(Point3d pt)
        {
            return new PVector3D(pt.X, pt.Y, pt.Z);
        }

        public static PVector3D ToPVector3D(Vector3d vec)
        {
            return new PVector3D(vec.X, vec.Y, vec.Z);
        }

        public static PVector2Di ToPVector2Di(Vector2d vec)
        {
            return new PVector2Di((int)vec.X, (int)vec.Y);
        }
    }
}
