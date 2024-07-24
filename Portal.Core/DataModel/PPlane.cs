using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public class PPlane
    {
        public PVector3D Origin { get; set; }
        public PVector3D XAxis { get; set; }
        public PVector3D YAxis { get; set; }

        public PPlane(PVector3D origin, PVector3D xAxis, PVector3D yAxis)
        {
            Origin = origin;
            XAxis = xAxis;
            YAxis = yAxis;
        }
    }
}
