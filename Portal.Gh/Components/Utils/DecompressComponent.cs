using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Portal.Core.Compression;
using Portal.Gh.Common;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Utils
{
    public class DecompressComponent : GH_Component
    {
        #region Metadata

        public DecompressComponent()
            : base("Decompress Bytes", "BDecompress",
                "Decompress bytes back to string with gzip algorithm",
                Config.Category, Config.SubCat.Utils)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("0309a890-6f40-447f-81e5-7bec19bbe341");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Bytes to decompress", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("String", "Str", "Decompressed string data", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BytesGoo bytes = null;

            if (!DA.GetData(0, ref bytes)) return;
            byte[] decompressedBytes = GZip.Decompress(bytes.Value);
            if (decompressedBytes == null || decompressedBytes.Length == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to decompress data. Decompression resulted in 0 bytes.");
                return;
            }
            string decompressedData = Encoding.UTF8.GetString(decompressedBytes);

            DA.SetData(0, decompressedData);
        }
    }
}