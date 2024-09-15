using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Gh.Common;
using Portal.Gh.Params.Json;
using Grasshopper.Kernel.Types;
using Portal.Core.DataModel;
using Rhino;
using Rhino.DocObjects;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Linq;
using Rhino.UI;

namespace Portal.Gh.Components.Serialization
{
    public class ObjectMetaComponent : GH_Component
    {
        public ObjectMetaComponent()
            : base("Object Meta", "ObjMeta",
                "Get meta attributes about the referenced rhino object.\nAttach a `trigger` component for dynamic update.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.septenary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.ObjectMeta;
        public override Guid ComponentGuid => new Guid("00cc6587-c5b2-4215-846a-64b19225c8b4");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "G", "Geometry to get information from", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new JsonDictParam(), "Id", "I", "Rhino object basic identification information in JSON format", GH_ParamAccess.item);
            pManager.AddParameter(new JsonDictParam(), "Layer", "L", "Layer information in JSON format", GH_ParamAccess.item);
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
            var doc = RhinoDoc.ActiveDoc;
            var obj = doc.Objects.Find(inputGoo.ReferenceID);
            if (obj == null) return;

            JsonDict objDict = GetObjectInfoDict(obj);
            JsonDict matDict = GetMaterialDict(obj, doc);
            JsonDict layerDict = GetLayerInfoDict(obj, doc);


            DA.SetData(0, new JsonDictGoo(objDict));
            DA.SetData(1, new JsonDictGoo(layerDict));
            DA.SetData(2, new JsonDictGoo(matDict));
        }


        private JsonDict GetObjectInfoDict(RhinoObject obj)
        {
            var objDict = new JsonDict()
            {
                {"Id", obj.Id},
                {"Name", obj.Name},
            };

            return objDict;
        }

        private JsonDict GetLayerInfoDict(RhinoObject obj, RhinoDoc doc)
        {
            var layer = doc.Layers.FindIndex(obj.Attributes.LayerIndex);
            
            JsonDict layerDict = new JsonDict
            {
                { "Id", layer.Id},
                { "FullPath", layer.FullPath},
                { "Color", new PColor(layer.Color).ToHex() },
            };

            int layerMaterialIndex = layer.RenderMaterialIndex;

            if (layerMaterialIndex >= 0)
            {
                var mat = doc.Materials[layerMaterialIndex];
                var matDict = GetMaterialDictForLayer(mat, doc);
                layerDict.Add("Material", matDict);
            }


            return layerDict;
        }

        private JsonDict GetMaterialDictForLayer(Material mat, RhinoDoc doc)
        {
            if (mat == null)
            {
                return null;
            }

            Texture[] textures = GetTexture(mat);

            PMaterial pMat = new PMaterial(mat.Name, new PColor(mat.DiffuseColor));
            List<PTexture> pTextures = textures.Select(tex => new PTexture(tex.FileName, (PTextureType)tex.TextureType)).ToList();
            pMat.Textures = pTextures;

            string matString = JsonConvert.SerializeObject(pMat);
            JsonDict matDict = JsonConvert.DeserializeObject<JsonDict>(matString);
            return matDict;
        }

        private JsonDict GetMaterialDict(RhinoObject obj, RhinoDoc doc)
        {
            Material mat = GetMaterial(obj, doc);
            if (mat == null)
            {
                return null;
            }
            Texture[] textures = GetTexture(mat);

            PMaterial pMat = new PMaterial(mat.Name, new PColor(mat.DiffuseColor));
            List<PTexture> pTextures = textures.Select(tex => new PTexture(tex.FileName, (PTextureType)tex.TextureType)).ToList();
            pMat.Textures = pTextures;

            string matString = JsonConvert.SerializeObject(pMat);
            JsonDict matDict = JsonConvert.DeserializeObject<JsonDict>(matString);
            return matDict;
        }

        private Material GetMaterial(RhinoObject obj, RhinoDoc doc)
        {
            int index = TryGetMeshMaterialIndex(obj);
            if (index < 0)
            {
                return null;
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