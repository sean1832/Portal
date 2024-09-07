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
using Portal.Gh.Params.Json;
using Portal.Gh.Params.Payloads;

namespace Portal.Gh.Components.Obsolete
{
    public class DeserializeMeshComponentV2_OBSOLETE : GH_Component
    {
        #region Metadata

        public DeserializeMeshComponentV2_OBSOLETE()
            : base("Deserialize Mesh", "DSrMesh",
                "Deserialize JSON string into meshes. This data can be read from communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { "desrmesh" };
        protected override Bitmap Icon => Icons.DeserializeMesh;
        public override Guid ComponentGuid => new Guid("46a68d71-9df0-4574-a9ef-4a7a9bb29b25");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PayloadParam(), "Payload", "P", "Payload packet to be deserialize", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Deserialized mesh", GH_ParamAccess.item);
            pManager.AddParameter(new JsonDictParam(), "Metadata", "#", "Metadata that describe the geometry",
                GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PayloadGoo payloadGoo = new PayloadGoo();

            if (!DA.GetData(0, ref payloadGoo)) return;

            var mesh = DeserializeMesh(payloadGoo.Value.Items.ToString());
            var meta = new JsonDictGoo(payloadGoo.Value.Meta);

            DA.SetData(0, mesh);
            DA.SetData(1, meta);
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