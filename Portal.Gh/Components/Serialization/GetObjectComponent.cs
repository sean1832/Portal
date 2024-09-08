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

namespace Portal.Gh.Components.Serialization
{
    public class GetObjectComponent : GH_Component
    {
        public GetObjectComponent()
            : base("Get Object", "RhObj",
                "Get information about the rhino object.\nAttach a `trigger` component for dynamic update.",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => null;
        public override Guid ComponentGuid => new Guid("00cc6587-c5b2-4215-846a-64b19225c8b4");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "G", "Geometry to get information from", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new JsonDictParam(), "Object", "O", "Rhino object information in JSON format", GH_ParamAccess.item);
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

            var objDict = new JsonDict()
            {
                {"Id", obj.Id},
                {"Name", obj.Name},
            };

            DA.SetData(0, new JsonDictGoo(objDict));
        }
    }
}