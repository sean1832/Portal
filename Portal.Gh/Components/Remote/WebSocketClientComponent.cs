using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Portal.Gh.Common;
using Portal.Core.Utils;
using Portal.Core.WebSocket;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Remote
{
    public class WebSocketClientComponent : GH_Component
    {
        private readonly WebSocketClientManager _socketClient;
        #region Metadata

        public WebSocketClientComponent()
            : base("WebSocket Client", "WS Client",
                "Client for sending data via WebSocket connection. Ideal for robust data communication with high data integrity. Slower than UDP." +
                "\n\nWebSocket:\n" +
                "Facilitates real-time, bidirectional communication with web applications. " +
                "WebSockets are less suited for high-speed requirements but provide robust, continuous data streams, " +
                "making them ideal for interactive applications requiring constant updates.",
                Config.Category, Config.SubCat.Remote)
        {
            Instances.DocumentServer.DocumentRemoved += OnDocumentClose;
            _socketClient = new WebSocketClientManager();
            _socketClient.Connected += (sender, args) => Message = "Connected";
            _socketClient.Disconnected += (sender, args) => Message = "Disconnected";
            _socketClient.Error += (sender, args) =>
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, args.Message);
                Message = "Error";
            };
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "wp client" };
        protected override Bitmap Icon => Icons.WebsocketClient;
        public override Guid ComponentGuid => new Guid("ab7b6872-2328-4667-a6fe-74b3fb61141b");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Host IP", "IP",
                "The IP address of the WebSocket server. (Default '127.0.0.1')", GH_ParamAccess.item, "127.0.0.1");

            pManager.AddIntegerParameter("Port", "Port",
                "The port number on which the WebSocket server is listening. (make sure this port is free)", GH_ParamAccess.item);

            pManager.AddTextParameter("Route", "Route", "Endpoint Route. format: '/route'\nIt specifies the path that the server will listen to for incoming WebSocket connections.", GH_ParamAccess.item, "/");

            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Message in bytes to send to server", GH_ParamAccess.item);

            pManager.AddBooleanParameter("Send", "Send",
                "Start sending message to server", GH_ParamAccess.item, false);

            pManager[3].Optional = true; // Message

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        #endregion

        private void OnDocumentClose(GH_DocumentServer sender, GH_Document doc)
        {
            Dispose();
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string hostIp = "";
            int port = 8765;
            string route = "";
            BytesGoo msg = null;
            bool connect = false;

            if (!DA.GetData(0, ref hostIp)) return;
            if (!DA.GetData(1, ref port)) return;
            if (!DA.GetData(2, ref route)) return;
            DA.GetData(3, ref msg);
            if (!DA.GetData(4, ref connect)) return;

            (bool isValid, string validatorMessage) = NetworkInputValidator.IsEndpointValid(route);
            if (!isValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Invalid Route: {validatorMessage}");
                Message = "Invalid route";
                return;
            }

            PortChecker portChecker = new PortChecker();
            if (!portChecker.IsPortAvailable(port))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Invalid port ({port}): {portChecker.GetPortDescription(port)}");
                return;
            }

            string uri = $"ws://{hostIp}:{port}{route}";
            Message = uri;

            if (connect && !_socketClient.IsConnected)
            {
                _socketClient.Connect(uri);
            }
            else if (!connect && _socketClient.IsConnected)
            {
                _socketClient.Disconnect();
            }

            if (msg != null && msg.Value.Length != 0)
            {
                _socketClient.Send(msg.Value);
            }
        }

        private void Dispose()
        {
            if (_socketClient != null && _socketClient.IsConnected)
            {
                _socketClient.Disconnect();
            }
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            Dispose();
            base.RemovedFromDocument(document);
        }
    }
}