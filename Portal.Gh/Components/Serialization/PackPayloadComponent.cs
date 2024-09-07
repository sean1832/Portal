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
    public class PackPayloadComponent : GH_Component
    {
        public PackPayloadComponent()
            : base("Pack Payload", "Pack",
                "Pack a list of payloads into a single string with metadata",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.PackPayload;
        public override Guid ComponentGuid => new Guid("1d0759eb-5dc5-4951-a429-3cfea458fd36");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new PayloadParam(), "Payloads", "P", "Payloads to be pack into a JSON string", GH_ParamAccess.list);
            pManager.AddParameter(new JsonDictParam(), "Metadata", "#", "(Optional) Metadata that describe the payload.",
                GH_ParamAccess.item);

            pManager[1].Optional = true;
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
            var metadataGoo = new JsonDictGoo();


            if (!DA.GetDataList(0, payloadGoos)) return;
            DA.GetData(1, ref metadataGoo);

            var payloads = payloadGoos.Select(payloadGoo => payloadGoo.Value).ToList();
            Payload collection = new Payload(payloads, metadataGoo.Value);

            string json = _isBeautify
                ? JsonConvert.SerializeObject(collection, Formatting.Indented)
                : JsonConvert.SerializeObject(collection);

            DA.SetData(0, json);
        }
    }
}