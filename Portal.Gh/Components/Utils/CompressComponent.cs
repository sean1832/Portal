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
    public class CompressComponent : GH_Component
    {
        #region Metadata

        public CompressComponent()
            : base("Compress Bytes", "CompBytes",
                "Compress bytes with gzip algorithm",
                Config.Category, Config.SubCat.Utils)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("2569a8ae-8722-47bb-8843-c00a111e5096");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Bytes to decompress", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Compressed Bytes", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BytesGoo data = null;

            if (!DA.GetData(0, ref data)) return;

            byte[] compressedBytes = GZip.Compress(data.Value);
            BytesGoo bytes = new BytesGoo(compressedBytes);
            float compressionRate = ((float)compressedBytes.Length / data.Value.Length)*100; // in percentage
            // round to 2 decimal places
            compressionRate = (float)Math.Round(compressionRate, 2);
            Message = $"Compression: {compressionRate}%";

            DA.SetData(0, bytes);
        }
    }
}