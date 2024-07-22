using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NetworkGh.Components.Utils
{
    public class DecodeBytesComponent : GH_Component
    {
        #region Metadata

        public DecodeBytesComponent()
            : base("DecodeBytesComponent", "Nickname",
                "Description",
                "Category", "SubCategory")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("92b0f16a-dbc5-4fc8-8a52-29ae24d73784");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }
    }
}