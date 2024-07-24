using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Gh.Common;

namespace Portal.Gh.Components.Utils
{
    public class DataSizeComponent : GH_Component
    {
        #region Metadata

        public DataSizeComponent()
            : base("Data Size", "Size",
                "Get data sizes information",
                Config.Category, Config.SubCat.Utils)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("1613e99f-fc33-474c-b9a2-16209e17e1cd");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Data", "Data", "Data to check", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Size", "Size", "Size of the data in bytes", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string data = string.Empty;

            if (!DA.GetData(0, ref data)) return;

            DA.SetData(0, data.Length);
        }
    }
}