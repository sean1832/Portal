﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public class PMesh : PEntity
    {
        public List<PVector3Df> Vertices { get; private set; }
        //public List<PVector3Df> Normals { get; private set; }
        public List<int[]> Faces { get; private set; }
        public List<string> VertexColors { get; private set; }
        public List<PVector2Df> UVs { get; private set; }

        public PMesh() : base(PGeoType.Mesh)
        {
            Vertices = new List<PVector3Df>();
            Faces = new List<int[]>();
            VertexColors = new List<string>();
            UVs = new List<PVector2Df>();
        }

        public PMesh(List<PVector3Df> vertices, List<int[]> faces) : base(PGeoType.Mesh)
        {
            Vertices = vertices;
            Faces = faces;
            VertexColors = new List<string>();
            UVs = new List<PVector2Df>();
        }

        public PMesh(List<PVector3Df> vertices, List<int[]> faces, List<string> vertexColors) : base(PGeoType.Mesh)
        {
            Vertices = vertices;
            Faces = faces;
            VertexColors = vertexColors;
            UVs = new List<PVector2Df>();
        }
        public PMesh(List<PVector3Df> vertices, List<int[]> faces, List<string> vertexColors, List<PVector2Df> uvs) : base(PGeoType.Mesh)
        {
            Vertices = vertices;
            Faces = faces;
            VertexColors = vertexColors;
            UVs = uvs;
        }
    }
}
