using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using Portal.Core.DataModel;
using Portal.Gh.Common;

namespace Portal.Gh.Components.Serialization
{
    public class DeserializeMeshComponent : GH_Component
    {
        #region Metadata

        public DeserializeMeshComponent()
            : base("Deserialize Mesh", "DSrMesh",
                "Deserialize JSON string into meshes. This data can be read from communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override IEnumerable<string> Keywords => new string[] { "desrmesh" };
        protected override Bitmap Icon => Icons.DeserializeMesh;
        public override Guid ComponentGuid => new Guid("202e2e4e-e3f6-40d5-8a5a-0b59c6ecb401");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Json data", "Json", "Json data to deserialize", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Deserialized mesh", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string data = string.Empty;

            if (!DA.GetData(0, ref data)) return;

            var mesh = DeserializeMesh(data);
            DA.SetData(0, mesh);
        }


        private Mesh DeserializeMesh(string data)
        {
            PMesh dataMesh = JsonConvert.DeserializeObject<PMesh>(data);
            if (dataMesh == null)
            {
                throw new InvalidOperationException("No meshes found in the JSON data.");
            }

            Mesh mesh = new Mesh();
            mesh.Vertices.AddVertices(dataMesh.Vertices.Select(vertex => new Point3d(vertex.X, vertex.Y, vertex.Z)));
            mesh.Faces.AddFaces(dataMesh.Faces.Select(face => new MeshFace(face[0], face[1], face[2], face[3])).ToArray());
            foreach (var hexColor in dataMesh.VertexColors)
            {
                PColor pColor = PColor.FromHexColor(hexColor);
                mesh.VertexColors.Add(pColor.R, pColor.G, pColor.B);
            }
            mesh.Normals.ComputeNormals();
            mesh.FaceNormals.ComputeFaceNormals();

            return mesh;
        }

    }
}