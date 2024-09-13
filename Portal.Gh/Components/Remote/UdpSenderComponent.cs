using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Portal.Gh.Common;
using Portal.Core;
using Portal.Core.Binary;
using Portal.Core.Udp;
using Portal.Core.Utils;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Remote
{
    public class UdpSenderComponent : GH_Component
    {
        #region Metadata

        public UdpSenderComponent()
            : base("UDP Sender", "<UDP>",
                "Sends data packets to a server via UDP. Recommended for packets smaller than 1472 bytes to avoid fragmentation." +
                "\n\nUDP:\n" +
                "Offers fast data transmission via UDP, prioritizing speed over reliability. " +
                "Best for scenarios where occasional data loss is acceptable, " +
                "UDP Sockets are not connection-oriented, hence faster but less reliable compared to TCP-based methods.",
                Config.Category, Config.SubCat.Remote)
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
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Message data in bytes to send", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Send", "Send", "Start sending data to server", GH_ParamAccess.item,
                false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        #endregion

        private ushort _lastChecksum;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string ip = null;
            int port = 0;
            BytesGoo message = null;
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

            if (IsDataExceeded(message.Value))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Data exceeds max size of 1472 bytes. Consider using Websockets instead to avoid fragmentation.");
                return;
            }


            if (!send) return;
            try
            {
                ushort checksum = Packet.Deserialize(message.Value).Header.Checksum;
                if (checksum != 0 && checksum == _lastChecksum) return; // Skip if the message is the same

                using (var udpClient = new UdpClientManager(ip, port))
                {
                    udpClient.Send(message.Value);
                    Message = $"Sent to {ip}:{port}";
                    udpClient.Error += (sender, e) =>
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                        Message = "Error";
                    };
                    _lastChecksum = checksum;
                } // properly dispose the client after sending the message
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                Message = "Error";
            }
        }

        private bool IsDataExceeded(byte[] data)
        {
            return data.Length > 1472;
        }
    }
}