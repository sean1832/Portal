using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public enum PType
    {
        Undefined,
        Mesh,
        Curve,
        Vector3D,
        Vector2D,
        Color,
        Plane
    }

    public enum CurveType
    {
        Base,
        Line,
        Arc,
        Circle,
        Polyline,
        Nurbs,
    }
}
