using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Portal.Gh.Common;
using Portal.Gh.Params.Payloads;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json;
using Portal.Core.DataModel;
using Portal.Gh.Params.Json;
using Rhino;
using Rhino.DocObjects;
using TextureType = Portal.Core.DataModel.TextureType;

namespace Portal.Gh.Components.Serialization
{
    public class GetMaterialComponent : GH_Component
    {
        public GetMaterialComponent()
            : base("Get Material", "Material",
                "Get reference object material information.\nAttach a `trigger` component for continues update.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.septenary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.GetMaterial;
        public override Guid ComponentGuid => new Guid("4a47e371-b922-43fc-a5cb-8cd2137819ee");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "G", "Geometry to get material from", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new JsonDictParam(), "Material", "M", "Material information in JSON format", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            IGH_GeometricGoo inputGoo = null;

            if (!DA.GetData(0, ref inputGoo)) return;

            if (inputGoo.ReferenceID == Guid.Empty)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Object is not referenced.");
                return;
            }

            switch (inputGoo)
            {
                case GH_Point pointGoo:
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Point is not supported.");
                    return;
                case GH_Curve curveGoo:
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Curve is not supported.");
                    return;
                default:
                    Material mat = GetMaterial(inputGoo.ReferenceID);
                    Texture[] textures = GetTexture(mat);

                    PMaterial pMat = new PMaterial(mat.Name, new PColor(mat.DiffuseColor));
                    List<PTexture> pTextures = textures.Select(tex => new PTexture(tex.FileName, (TextureType)tex.TextureType)).ToList();
                    pMat.Textures = pTextures;

                    string matString = JsonConvert.SerializeObject(pMat);
                    JsonDict matDict = JsonConvert.DeserializeObject<JsonDict>(matString);

                    DA.SetData(0, new JsonDictGoo(matDict));
                    break;
            }
        }

        private Material GetMaterial(Guid id)
        {
            var doc = RhinoDoc.ActiveDoc;
            var obj = doc.Objects.Find(id);
            if (obj == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Object not found.");
            }
            int index = TryGetMeshMaterialIndex(obj);
            if (index < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, $"Object has no material assigned.");
            }
            return doc.Materials[index];
        }

        private Texture[] GetTexture(Material mat)
        {
            return mat.GetTextures();
        }

        private int TryGetMeshMaterialIndex(RhinoObject obj)
        {
            if (obj == null)
            {
                return -1;
            }

            int index = obj.Attributes.MaterialIndex;
            return index;
        }
    }
}