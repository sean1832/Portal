using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public class PMesh
    {
        public List<PVector3Df> Vertices { get; set; }
        public List<PVector3Df> Normals { get; set; }
        public List<int[]> Faces { get; set; }
        public List<PVector2Df> UVs { get; set; }
    }
}
