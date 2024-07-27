using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public class PMesh
    {
        public List<PVector3Df> Vertices { get; private set; }
        public List<PVector3Df> Normals { get; private set; }
        public List<int[]> Faces { get; private set; }
        public List<PVector2Df> UVs { get; private set; }

        public PMesh(List<PVector3Df> vertices, List<PVector3Df> normals, List<int[]> faces, List<PVector2Df> uVs)
        {
            Vertices = vertices;
            Normals = normals;
            Faces = faces;
            UVs = uVs;
        }
    }
}
