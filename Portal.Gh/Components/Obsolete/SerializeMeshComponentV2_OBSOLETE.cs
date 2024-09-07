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
using Rhino.Geometry.Collections;

namespace Portal.Gh.Components.Obsolete
{
    public class SerializeMeshComponentV2_OBSOLETE : GH_Component
    {
        #region Metadata

        public SerializeMeshComponentV2_OBSOLETE()
            : base("Serialize Mesh", "SrMesh",
                "Serialize meshes into a payload. This payload should be pack using `Pack Payload` before send over communication pipeline for data exchange.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.SerializeMesh;
        public override Guid ComponentGuid => new Guid("a6d957ac-89ae-420d-ad0f-3c0fa516251f");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to serialize", GH_ParamAccess.item);
            pManager.AddParameter(new JsonDictParam(), "Metadata", "#", "(Optional) Metadata that describe the geometry",
                GH_ParamAccess.item);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new PayloadParam(), "Payload", "P", "Structured packet payload", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            JsonDictGoo metaGoo = new JsonDictGoo();

            if (!DA.GetData(0, ref mesh)) return;
            DA.GetData(1, ref metaGoo);

            JsonDictGoo meshGoo = SerializeMesh(mesh);
            PayloadGoo payloadGoo = new PayloadGoo(new Payload(meshGoo.Value, metaGoo.Value));

            DA.SetData(0, payloadGoo);
        }

        private JsonDictGoo SerializeMesh(Mesh mesh)
        {
            var vertices = mesh.Vertices.Select(vertex => new PVector3Df(vertex.X, vertex.Y, vertex.Z)).ToList();
            var faces = mesh.Faces.Select(face => new[] { face.A, face.B, face.C, face.D }).ToList();
            var vertexColors = mesh.VertexColors.Select(color =>
                {
                    var pColor = new PColor(color.R, color.G, color.B, color.A);
                    return pColor.ToHex();
                })
                .ToList();
            PMesh meshModel = new PMesh(vertices, faces, vertexColors);

            string jsonString = JsonConvert.SerializeObject(meshModel);
            JsonDict dict = JsonConvert.DeserializeObject<JsonDict>(jsonString);
            return new JsonDictGoo(dict);
        }
    }
}