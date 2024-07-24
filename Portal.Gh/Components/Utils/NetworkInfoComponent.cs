using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Gh.Common;
using Portal.Core.Utils;

namespace Portal.Gh.Components.Utils
{
    public class NetworkInfoComponent : GH_Component
    {
        #region Metadata

        public NetworkInfoComponent()
            : base("Network Info", "Info",
                "Retrieves and displays various network information from the local machine, including IP address, MAC address, and network interface details.",
                Config.Category, Config.SubCat.Utils)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.NetworkInfo;
        public override Guid ComponentGuid => new Guid("00ba88e7-e038-471f-ac2f-16d0f2150414");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Properties", "Prop", "Adaptor Properties as JSON. Deserialize to extract info.", GH_ParamAccess.list);
            pManager.AddTextParameter("Local IP", "IP", "Local IP address of this machine", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            NetworkInfoManager manager = new NetworkInfoManager();
            List<string> properties = new List<string>();
            string ip = null;

            // Load all network info
            manager.Load();

            foreach (var network in manager.Networks)
            {
                properties.Add(network.ToJson());
                if (network.IsPrimary) ip = network.IpAddress;
            }
            

            DA.SetDataList(0, properties);
            DA.SetData(1, ip);
        }
    }
}