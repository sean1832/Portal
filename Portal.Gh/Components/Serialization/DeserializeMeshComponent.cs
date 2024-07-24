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
            : base("Deserialize Mesh", "DSrlzMesh",
                "Deserialize JSON string into meshes. This data can be read from communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("a96a0acf-54de-49cc-88d6-bf76b6844d5d");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Json data", "Json", "Json data to deserialize", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Deserialized mesh", GH_ParamAccess.list);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string data = string.Empty;

            if (!DA.GetData(0, ref data)) return;

            var meshes = DeserializeMesh(data);

            DA.SetDataList(0, meshes);
        }

        private List<Mesh> DeserializeMesh(string data)
        {
            List<Mesh> meshes = new List<Mesh>();

            List<PMesh> dataMeshes = JsonConvert.DeserializeObject<List<PMesh>>(data);

            foreach (var dataMesh in dataMeshes)
            {
                Mesh mesh = new Mesh();
                mesh.Vertices.AddVertices(dataMesh.Vertices.Select(vertex => new Point3d(vertex.X, vertex.Y, vertex.Z)));
                mesh.Faces.AddFaces(dataMesh.Faces.Select(face => new MeshFace(face[0], face[1], face[2], face[3])).ToArray());
                mesh.TextureCoordinates.AddRange(dataMesh.UVs.Select(uv => new Point2f(uv.X, uv.Y)).ToArray());
                mesh.Normals.AddRange(dataMesh.Normals.Select(normal => new Vector3f(normal.X, normal.Y, normal.Z))
                    .ToArray());

                meshes.Add(mesh);
            }

            return meshes;
        }
    }
}