using Grasshopper;
using Grasshopper.Kernel;
using NetworkGh.Common;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using NetworkGh.Core.NamedPipe;
using NetworkGh.Core;
using Rhino;
using System.Text;

namespace NetworkGh.Components.Local
{
    public class NamedPipeReceiverComponent : GH_Component
    {
        private string _lastReceivedMessage;
        private NamedPipeServer _server;

        #region Metadata

        public NamedPipeReceiverComponent()
            : base("Named Pipe Receiver", ">pipe<",
                "Server that listens for messages sent to a named pipe." +
                "\n\nNamed Pipes:\n" +
                "A fast inter-process communication (IPC) method for local machine communication.\n" +
                "Faster than WebSocket, UDP, and TCP, but limited to local use.",
                Config.Category, Config.SubCat.LocalIpc)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "np receiver", "np server", "pipe receiver" };
        protected override Bitmap Icon => Icons.PipeReceiver;
        public override Guid ComponentGuid => new Guid("9903815c-5f6e-4341-b41b-278d3051730e");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pipe Name", "Pipe", "The unique identifier for the named pipe. This name is used by both the server and clients to connect.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Buffer Size", "BufSize", "The size of the buffer used for each read operation, in bytes. Default is 4096 bytes.", GH_ParamAccess.item, 4096);
            pManager.AddBooleanParameter("Start", "Start", "Set to true to start the server and begin listening on the specified pipe.", GH_ParamAccess.item,
                false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Data", "Data", "Data received from client", GH_ParamAccess.item);
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string pipeName = null;
            int bufferSize = 0;
            bool start = false;

            if (!DA.GetData(0, ref pipeName)) return;
            if (!DA.GetData(1, ref bufferSize)) return;
            if (!DA.GetData(2, ref start)) return;

            if (start && _server == null)
            {
                // Initializes and starts the server if not already started
                StartServer(pipeName, bufferSize);
            }
            else if (!start)
            {
                // Stops the server if requested
                DisposeServer();
                Message = "Stopped";
            }

            // Always set the last received message to output, even if it's null
            DA.SetData(0, _lastReceivedMessage);

        }


        private void StartServer(string pipeName, int bufferSize)
        {
            DisposeServer(); // Ensure any previous server is disposed

            void HandleError(Exception ex)
            {
                RhinoApp.InvokeOnUiThread((Action)delegate
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Error: {ex.Message}");
                });
            }

            void HandleMessage(byte[] data)
            {
                RhinoApp.InvokeOnUiThread((Action)delegate
                {
                    _lastReceivedMessage = Encoding.UTF8.GetString(data);
                    ExpireSolution(true);
                });
            }

            try
            {
                _server = new NamedPipeServer(pipeName, bufferSize, HandleError, HandleMessage);
                _server.Start();
                Message = "Listening";
            }
            catch (Exception e)
            {
                HandleError(e);
            }
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