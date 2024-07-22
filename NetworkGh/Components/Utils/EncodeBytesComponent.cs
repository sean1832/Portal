using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NetworkGh.Components.Utils
{
    public class EncodeBytesComponent : GH_Component
    {
        #region Metadata

        public EncodeBytesComponent()
            : base("EncodeBytesComponent", "Nickname",
                "Description",
                "Category", "SubCategory")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("02328f19-9f50-4d72-b0d7-25ba2c052d49");

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