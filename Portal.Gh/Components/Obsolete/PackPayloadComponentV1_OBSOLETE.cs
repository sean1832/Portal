using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Portal.Gh.Common;
using Newtonsoft.Json;
using Portal.Core.DataModel;
using Portal.Gh.Params.Json;
using GH_IO.Serialization;
using System.Windows.Forms;
using Portal.Gh.Params.Payloads;

namespace Portal.Gh.Components.Serialization
{
    public class PackPayloadComponentV1_OBSOLETE : GH_Component
    {
        public PackPayloadComponentV1_OBSOLETE()
            : base("Pack Payload", "Pack",
                "Description",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        #region Meta

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.PackPayload;
        public override Guid ComponentGuid => new Guid("3ae8f342-1397-4f68-a15a-859a48bb3316");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PayloadParam(), "Payloads", "P", "Payloads to be pack into a JSON string", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("JsonString", "Txt", "Serialized JSON String", GH_ParamAccess.item);
        }

        #endregion

        #region (De)Serialization

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("_isBeautify", _isBeautify);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            _isBeautify = reader.GetBoolean("_isBeautify");
            return base.Read(reader);
        }

        #endregion


        #region Context Menu

        private bool _isBeautify;

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem menuItem = new ToolStripMenuItem("Indent Json", null, ToggleBeautify, Keys.None)
            {
                Checked = _isBeautify,
                ToolTipText = @"Convert JSON into readable format. Will significantly increase binary size."
            };
            menu.Items.Add(menuItem);
        }

        private void ToggleBeautify(object sender, EventArgs e)
        {
            _isBeautify = !_isBeautify;
            ExpireSolution(true);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var payloadGoos = new List<PayloadGoo>();

            if (!DA.GetDataList(0, payloadGoos)) return;

            var payloads = payloadGoos.Select(payloadGoo => payloadGoo.Value).ToList();

            string json = _isBeautify
                ? JsonConvert.SerializeObject(payloads, Formatting.Indented)
                : JsonConvert.SerializeObject(payloads);

            DA.SetData(0, json);
        }
    }
}