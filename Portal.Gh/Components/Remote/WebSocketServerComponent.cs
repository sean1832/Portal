using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Core.Utils;
using Portal.Gh.Common;
using Portal.Gh.Components.Remote.Behavior;
using Portal.Gh.Params.Bytes;
using WebSocketSharp.Server;

namespace Portal.Gh.Components.Remote
{
    public class WebSocketServerComponent : GH_Component
    {
        private byte[] _lastReceivedMessage = Array.Empty<byte>();
        private WebSocketServer _server;

        #region Metadata

        public WebSocketServerComponent()
            : base("WebSocket Server", "WS Server",
                "Server for receiving data from client via WebSocket connection. Ideal for robust data communication with high data integrity. Slower than UDP."+
                "\n\nWebSocket:\n" +
                "Facilitates real-time, bidirectional communication with web applications. " +
                "WebSockets are less suited for high-speed requirements but provide robust, continuous data streams, " +
                "making them ideal for interactive applications requiring constant updates.",
                Config.Category, Config.SubCat.Remote)
        {
            Instances.DocumentServer.DocumentRemoved += OnDocumentClose;
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "wp server" };
        protected override Bitmap Icon => Icons.WebsocketServer;
        public override Guid ComponentGuid => new Guid("50f302a5-ba3d-4f2b-aa3a-06c8408b990a");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            
            pManager.AddIntegerParameter("Port", "Port", "The port number on which the WebSocket server is listening. (make sure this port is free)", GH_ParamAccess.item);
            pManager.AddTextParameter("Route", "Route", "Endpoint Route. format: '/route'\nIt specifies the path that the server will listen to for incoming WebSocket connections.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Start", "Start", "Start the server", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Message received in bytes", GH_ParamAccess.item);
        }

        #endregion

        private void OnDocumentClose(GH_DocumentServer sender, GH_Document doc)
        {
            StopServer();
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string route = "";
            int port = 0;
            bool start = false;

            if (!DA.GetData(0, ref port)) return;
            if (!DA.GetData(1, ref route)) return;
            if (!DA.GetData(2, ref start)) return;

            (bool isValid, string validatorMessage) = NetworkInputValidator.IsEndpointValid(route);
            if (!isValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Invalid Route: {validatorMessage}");
                Message = "Invalid route";
                return;
            }

            if (start)
            {
                StartServer(port, route);
            }
            else
            {
                StopServer();
            }

            BytesGoo outputGoo = new BytesGoo(_lastReceivedMessage);

            DA.SetData(0, outputGoo);
        }

        private void StartServer(int port, string route)
        {
            try
            {
                if (_server == null)
                {
                    _server = new WebSocketServer(port);
                    _server.AddWebSocketService<GetBehavior>(route);

                    GetBehavior.MessageReceived += (args) =>
                    {
                        _lastReceivedMessage = args;
                        ExpireSolution(true);
                    };

                    GetBehavior.ErrorOccured += (args) =>
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, args);
                    };
                }

                _server.Start();
                Message = "Started";
            }
            catch (Exception e)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                Message = "Error starting server";
            }
        }

        private void StopServer()
        {
            if (_server != null)
            {
                _server.Stop();
                _server = null;
                Message = "Stopped";
            }
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            StopServer();
            base.RemovedFromDocument(document);
        }
    }
}