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
        //public List<PVector3Df> Normals { get; private set; }
        public List<int[]> Faces { get; private set; }
        //public List<PVector2Df> UVs { get; private set; }
        public List<string> VertexColors { get; private set; }

        public PMesh()
        {
            Vertices = new List<PVector3Df>();
            Faces = new List<int[]>();
            VertexColors = new List<string>();
        }

        public PMesh(List<PVector3Df> vertices, List<int[]> faces)
        {
            Vertices = vertices;
            Faces = faces;
            VertexColors = new List<string>();
        }

        public PMesh(List<PVector3Df> vertices, List<int[]> faces, List<string> vertexColors)
        {
            Vertices = vertices;
            Faces = faces;
            VertexColors = vertexColors;
        }
    }
}
