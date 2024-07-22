using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using WebSocketSharp;
using NetworkGh.Common;
using NetworkGh.Core.WebSocket;
using NetworkGh.Core.Utils;

namespace NetworkGh.Components.Remote
{
    public class WebSocketPeerComponent : GH_Component
    {
        private readonly WebSocketManager _socket;
        private string _lastReceivedMessage;
        #region Metadata

        public WebSocketPeerComponent()
            : base("WebSocket Peer", "websocket",
                "Establishes and manages a WebSocket connection, allowing for real-time data communication over the Web." +
                "\n\nWebSocket:\n" +
                "A protocol for real-time, two-way communication over the Web.\n" +
                "It establishes persistent connections, enabling low-latency, full-duplex interaction.\n" +
                "Suitable for applications requiring real-time updates and continuous data exchange.",
                Config.Category, Config.SubCat.RemoteIpc)
        {
            _socket = new WebSocketManager();
            _socket.MessageReceived += (sender, msg) =>
            {
                _lastReceivedMessage = msg;
                // Expire the solution to recompute the component on message received
                Rhino.RhinoApp.InvokeOnUiThread((Action)delegate
                {
                    ExpireSolution(true);
                });
            };
            _socket.Connected += (sender, args) => Message = "Connected";
            _socket.Disconnected += (sender, args) => Message = "Disconnected";
            _socket.Error += (sender, args) =>
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, args.Message);
                Message = "Error";
            };
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.websocket;
        public override Guid ComponentGuid => new Guid("ab7b6872-2328-4667-a6fe-74b3fb61141b");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Host IP", "IP",
                "The IP address of the WebSocket server. (Default '127.0.0.1')", GH_ParamAccess.item, "127.0.0.1");

            pManager.AddIntegerParameter("Port", "Port",
                "The port number on which the WebSocket server is listening. (make sure this port is free)", GH_ParamAccess.item);

            pManager.AddTextParameter("Message", "msg", "Message to send to server (optional)", GH_ParamAccess.item);

            pManager.AddBooleanParameter("Connect", "Conn",
                "Set to true to initiate the connection. Set to false to disconnect.", GH_ParamAccess.item, false);

            pManager[2].Optional = true;

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("output", "out", "Output received from the WebSocket server.", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string hostIp = "";
            int port = 8765;
            string msg = "";
            bool connect = false;

            if (!DA.GetData(0, ref hostIp)) return;
            if (!DA.GetData(1, ref port)) return;
            DA.GetData(2, ref msg);
            if (!DA.GetData(3, ref connect)) return;

            PortChecker portChecker = new PortChecker();
            if (!portChecker.IsPortAvailable(port))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Invalid port ({port}): {portChecker.GetPortDescription(port)}");
                return;
            }

            string uri = $"ws://{hostIp}:{port}";
            Message = uri;

            if (connect && !_socket.IsConnected)
            {
                _socket.Connect(uri);
            }
            else if (!connect && _socket.IsConnected)
            {
                _socket.Disconnect();
            }

            if (!string.IsNullOrEmpty(msg))
            {
                _socket.Send(msg);
            }

            DA.SetData(0, _lastReceivedMessage);
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            if (_socket != null && _socket.IsConnected)
            {
                _socket.Disconnect();
            }
            base.RemovedFromDocument(document);
        }
    }
}