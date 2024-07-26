using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Portal.Core.DataModel;
using Portal.Gh.Common;

namespace Portal.Gh.Components.Serialization
{
    public class SerializeMeshComponent : GH_Component
    {
        #region Metadata

        public SerializeMeshComponent()
            : base("Serialize Mesh", "SrMesh",
                "Serialize meshes into a JSON representation. This data can be send over communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("d38b8d93-833e-4b9d-bf76-1376feab347c");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to serialize", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("JsonData", "Json", "Serialized curve as JSON", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> mesh = new List<Mesh>();

            if (!DA.GetDataList(0, mesh)) return;

            DA.SetData(0, SerializeMesh(mesh));
        }

        private string SerializeMesh(List<Mesh> meshes)
        {
            List<PMesh> meshesModel = new List<PMesh>();
            foreach (var mesh in meshes)
            {
                PMesh meshModel = new PMesh()
                {
                    Vertices = mesh.Vertices.Select(vertex => new PVector3Df(vertex.X, vertex.Y, vertex.Z)).ToList(),
                    Faces = mesh.Faces.Select(face => new[] {face.A, face.B, face.C, face.D}).ToList(),
                    Normals = mesh.Normals.Select(normal => new PVector3Df(normal.X, normal.Y, normal.Z)).ToList(),
                    UVs = mesh.TextureCoordinates.Select(uv => new PVector2Df(uv.X, uv.Y)).ToList()
                };

                meshesModel.Add(meshModel);
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(meshesModel);
        }
    }
}