using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using NetworkGh.Common;
using NetworkGh.Core;
using Rhino;
using NetworkGh.Core.Udp;

namespace NetworkGh.Components.Remote
{
    public class UdpReceiverComponent : GH_Component
    {
        private UdpServerManager _server;
        private string _lastReceivedMessage;

        #region Metadata

        public UdpReceiverComponent()
            : base("Udp Receiver", ">UDP<",
                "Receives data packets from a UDP client. Recommended for packets smaller than 1472 bytes." +
                "\n\nUDP (User Datagram Protocol):\n" +
                "A fast, efficient protocol for sending messages (datagrams) between devices and processes without prior connection.\n" +
                "Ideal for low-latency applications, it doesn't guarantee order, error-checking, or delivery acknowledgment.\n" +
                "Suitable for both internet and local machine communication where speed is critical and occasional data loss is acceptable.",
                Config.Category, Config.SubCat.RemoteIpc)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.secondary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.UDPReceiver;
        public override Guid ComponentGuid => new Guid("48f92736-b8eb-4d4b-9455-870c36abbe6d");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Port", "Port", "Port to listen from (make sure this port is free)", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Start", "Start", "Start listening", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Data", "Data", "Data received from client", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int port = 0;
            bool start = false;

            if (!DA.GetData(0, ref port)) return;
            if (!DA.GetData(1, ref start)) return;

            try
            {
                if (start)
                {
                    Message = "Listening";
                    if (_server == null)
                    {
                        _server = new UdpServerManager(port);
                        _server.StartListening((data, endPoint) =>
                        {
                            Message = "Data received";
                            RhinoApp.InvokeOnUiThread((Action)delegate
                            {
                                if (_server != null)
                                {
                                    _lastReceivedMessage = Encoding.UTF8.GetString(data);
                                    ExpireSolution(true);
                                }
                            });
                        });
                        _server.Error += (sender, args) =>
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, args.Message);
                            Message = "Error";
                            DisposeServer();
                        };
                    }
                }
                else
                {
                    Message = "Disconnected";
                    DisposeServer();
                }
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error starting server: " + e.Message);
                DisposeServer();
            }

            DA.SetData(0, _lastReceivedMessage);
        }

        private void DisposeServer()
        {
            if (_server == null) return;
            _server?.Dispose();
            _server = null;
        }

        // Dispose the server when the component is removed from the document
        public override void RemovedFromDocument(GH_Document document)
        {
            DisposeServer();
            base.RemovedFromDocument(document);
        }
    }
}