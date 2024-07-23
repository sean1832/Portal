using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using NetworkGh.Common;
using NetworkGh.Core.Utils;
using NetworkGh.Core.WebSocket.Behavior;
using WebSocketSharp.Server;

namespace NetworkGh.Components.Remote
{
    public class WebSocketServerComponent : GH_Component
    {
        private string _lastReceivedMessage;
        private WebSocketServer _server;

        #region Metadata

        public WebSocketServerComponent()
            : base("WebSocket Server", "WS Server",
                "Server for receiving data from client via WebSocket connection. Ideal for robust data communication with high data integrity. Slower than UDP."+
                "\n\nWebSocket:\n" +
                "A communication protocol enabling real-time, two-way interaction over the web. (Also work in local machine)\n" +
                "It establishes persistent connections, making it suitable for environments where reliable data exchange is critical.",
                Config.Category, Config.SubCat.RemoteIpc)
        {
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
            pManager.AddTextParameter("Message", "msg", "Message received", GH_ParamAccess.item);
        }

        #endregion
        
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
            DA.SetData(0, _lastReceivedMessage);
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