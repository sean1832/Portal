using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Gh.Common;
using Newtonsoft.Json;
using Portal.Core.DataModel;
using Portal.Gh.Params.Json;
using GH_IO.Serialization;
using System.Windows.Forms;

namespace Portal.Gh.Components.Obsolete
{
    public class PackPayloadComponentV0_OBSOLETE : GH_Component
    {
        public PackPayloadComponentV0_OBSOLETE()
            : base("Pack Payload", "Pack",
                "Description",
                Config.Category, Config.SubCat.Serialization)
        {
        }

        #region Metadata

        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.PackPayload;
        public override Guid ComponentGuid => new Guid("7e14058d-78cd-4565-923a-d261f0b45e68");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new JsonDictParam(), "Data", "Data", "The actual data",
                GH_ParamAccess.list);
            pManager.AddParameter(new JsonDictParam(), "Metadata", "Meta", "Extra information about each entry of the serialized data",
                GH_ParamAccess.list);

            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Json String", "Txt", "Serialized JSON String", GH_ParamAccess.item);
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
            var dataGoo = new List<JsonDictGoo>();
            var metadataGoo = new List<JsonDictGoo>();

            if (!DA.GetDataList(0, dataGoo)) return;
            DA.GetDataList(1, metadataGoo);

            DA.SetData(0, PackPayload(dataGoo, metadataGoo));
        }

        private string PackPayload(List<JsonDictGoo> data, List<JsonDictGoo> metadataGoos)
        {
            List<Payload> payloads = new List<Payload>();
            JsonDict lastMetadata = null;
            for (int i = 0; i < data.Count; i++)
            {
                JsonDict metadata;
                try
                {
                    metadata = metadataGoos[i].Value;
                    lastMetadata = metadata;
                }
                catch (Exception e)
                {
                    metadata = lastMetadata;
                }

                var payload = new Payload
                {
                    Data = data[i].Value,
                    Metadata = metadata
                };
                payloads.Add(payload);
            }


            string json = _isBeautify
                ? JsonConvert.SerializeObject(payloads, Formatting.Indented)
                : JsonConvert.SerializeObject(payloads);
            return json;
        }
    }
}