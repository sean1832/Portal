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

namespace Portal.Gh.Components.Obsolete
{
    public class DecompressComponent_OBSOLETE : GH_Component
    {
        #region Metadata

        public DecompressComponent_OBSOLETE()
            : base("Decompress Bytes", "DeCB",
                "Decompress bytes back to string with gzip algorithm",
                Config.Category, Config.SubCat.Utils)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { "decb", "decompress b" };
        protected override Bitmap Icon => Icons.Decompress;
        public override Guid ComponentGuid => new Guid("0309a890-6f40-447f-81e5-7bec19bbe341");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Bytes to decompress", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Decompressed bytes", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BytesGoo bytes = null;

            if (!DA.GetData(0, ref bytes)) return;
            if (bytes.Value == null || bytes.Value.Length == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input data is empty.");
                return;
            }

            if (!GZip.IsGzipped(bytes.Value))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input data is not GZip compressed.");
                return;
            }

            byte[] decompressedBytes = GZip.Decompress(bytes.Value);
            if (decompressedBytes == null || decompressedBytes.Length == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to decompress data. Decompression resulted in 0 bytes.");
                return;
            }
            BytesGoo decompressedBytesGoo = new BytesGoo(decompressedBytes);
            DA.SetData(0, decompressedBytesGoo);
        }
    }
}