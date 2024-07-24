using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Portal.Gh.Common;
using Portal.Core;
using Portal.Core.Udp;
using Portal.Core.Utils;

namespace Portal.Gh.Components.Remote
{
    public class UdpSenderComponent : GH_Component
    {
        #region Metadata

        public UdpSenderComponent()
            : base("UDP Sender", "<UDP>",
                "Sends data packets to a server via UDP. Recommended for packets smaller than 1472 bytes." +
                "\n\nUDP:\n" +
                "Offers fast data transmission via UDP, prioritizing speed over reliability. " +
                "Best for scenarios where occasional data loss is acceptable, " +
                "UDP Sockets are not connection-oriented, hence faster but less reliable compared to TCP-based methods.",
                Config.Category, Config.SubCat.RemoteIpc)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.UDPSender;
        public override Guid ComponentGuid => new Guid("1a259c3e-d7d7-49bc-b7e6-54d3df3e7b02");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Server IP", "IP", "IP address of the target UDP server (default '127.0.0.1)", GH_ParamAccess.item,
                "127.0.0.1");
            pManager.AddIntegerParameter("Port", "Port", "Port number of the target UDP server (make sure this port is free)", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "msg", "Message to send", GH_ParamAccess.item);

            pManager.AddBooleanParameter("Send", "Send", "Start sending message to server", GH_ParamAccess.item,
                false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string ip = null;
            int port = 0;
            string message = null;
            bool send = false;

            if (!DA.GetData(0, ref ip)) return;
            if (!DA.GetData(1, ref port)) return;
            if (!DA.GetData(2, ref message)) return;
            if (!DA.GetData(3, ref send)) return;

            PortChecker portChecker = new PortChecker();
            if (!portChecker.IsPortAvailable(port))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Invalid port ({port}): {portChecker.GetPortDescription(port)}");
                return;
            }

            if (IsDataExceeded(message))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Data size is exceeded 1472 bytes. Consider using WebSocket instead.");
                return;
            }


            if (!send) return;
            try
            {
                using (var udpClient = new UdpClientManager(ip, port))
                {
                    udpClient.Send(Encoding.UTF8.GetBytes(message));
                    Message = $"Sent to {ip}:{port}";
                    udpClient.Error += (sender, e) =>
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                        Message = "Error";
                    };
                } // properly dispose the client after sending the message
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                Message = "Error";
            }
        }

        private bool IsDataExceeded(string data)
        {
            return Encoding.UTF8.GetByteCount(data) > 1472;
        }
    }
}