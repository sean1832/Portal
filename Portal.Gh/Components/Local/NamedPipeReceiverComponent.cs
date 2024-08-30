using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Core;
using Rhino;
using System.Text;
using Portal.Gh.Common;
using Portal.Core.NamedPipe;
using Portal.Gh.Components.Local.Behaviour;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Local
{
    public class NamedPipeReceiverComponent : GH_Component
    {
        private byte[] _lastReceivedMessage = Array.Empty<byte>();
        private NamedPipeServer _server;

        #region Metadata

        public NamedPipeReceiverComponent()
            : base("Pipe Receiver", ">pipe<",
                "Server that listens for messages sent to a named pipe.\n" +
                "\n\nNamed Pipes:\n" +
                "Provides reliable inter-process communication within the same machine, using stream-based data transfer. " +
                "Named Pipes are highly reliable and suitable for complex data exchanges within a single local machine, " +
                "but they do not support remote communication.",
                Config.Category, Config.SubCat.Local)
        {
            Instances.DocumentServer.DocumentRemoved += OnDocumentClose;
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "np receiver", "np server", "pipe receiver" };
        protected override Bitmap Icon => Icons.PipeReceiver;
        public override Guid ComponentGuid => new Guid("9903815c-5f6e-4341-b41b-278d3051730e");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pipe Name", "Name", "The unique identifier for the named pipe. This name is used by both the server and clients to connect.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Start", "Start", "Set to true to start the server and begin listening on the specified pipe.", GH_ParamAccess.item,
                false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Bytes received from client", GH_ParamAccess.item);
        }

        #endregion

        private void OnDocumentClose(GH_DocumentServer sender, GH_Document doc)
        {
            DisposeServer();
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string pipeName = null;
            bool start = false;

            if (!DA.GetData(0, ref pipeName)) return;
            if (!DA.GetData(1, ref start)) return;

            if (start && _server == null)
            {
                // Initializes and starts the server if not already started
                StartServer(pipeName, 4096);
            }
            else if (!start)
            {
                // Stops the server if requested
                DisposeServer();
                Message = "Stopped";
            }

            BytesGoo outputGoo = new BytesGoo(_lastReceivedMessage);

            // Always set the last received message to output, even if it's null
            DA.SetData(0, outputGoo);

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
                DisposeServer();
            }

            void HandleMessage(byte[] data)
            {
                RhinoApp.InvokeOnUiThread((Action)delegate
                {
                    _lastReceivedMessage = data;
                    ExpireSolution(true);
                });
            }

            try
            {
                _server = new NamedPipeServer(pipeName, bufferSize, HandleError, HandleMessage, new NamedPipeServerReceivedBehaviour());
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