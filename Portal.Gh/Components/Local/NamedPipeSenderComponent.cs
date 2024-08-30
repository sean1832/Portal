using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Portal.Gh.Common;
using Portal.Core.Utils;
using Portal.Core.NamedPipe;
using Portal.Gh.Params.Bytes;
using System.Windows.Forms;
using Portal.Core.DataModel;

namespace Portal.Gh.Components.Local
{
    public class NamedPipeSenderComponent : GH_Component
    {

        #region Metadata

        public NamedPipeSenderComponent()
            : base("Pipe Sender", "<pipe>",
                "Client that sends messages to a named pipe server.\n" +
                "\n\nNamed Pipes:\n" +
                "Provides reliable inter-process communication within the same machine, using stream-based data transfer. " +
                "Named Pipes are highly reliable and suitable for complex data exchanges within a single local machine, " +
                "but they do not support remote communication."
                , Config.Category, Config.SubCat.Local)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "np sender", "np client", "pipe sender" };
        protected override Bitmap Icon => Icons.PiperSender;
        public override Guid ComponentGuid => new Guid("43908314-80b3-438e-87c7-3ab78ed27816");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Pipe Name", "Name", "The unique identifier for the named pipe. This name is used by both the server and clients to connect.", GH_ParamAccess.item);
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "Data in bytes to be sent to the server via the named pipe.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Send", "Send", "Set to true to send the message to the server.", GH_ParamAccess.item,
                false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string pipeName = null;
            BytesGoo message = null;
            bool send = false;

            if (!DA.GetData(0, ref pipeName)) return;
            if (!DA.GetData(1, ref message)) return;
            if (!DA.GetData(2, ref send)) return;

            string serverName = "."; // Local machine

            if (send)
            {
                SendMessage(message.Value, serverName, pipeName);
            }
        }

        private void SendMessage(byte[] data, string serverName, string pipeName)
        {
            Task.Run(() =>
            {
                try
                {
                    using var client = new NamedPipeClient(serverName, pipeName, HandleError); // TODO: manually dispose
                    client.Connect();
                    client.SendAsync(data).Wait();
                    UpdateMessage($@"\\{serverName}\pipe\{pipeName}");
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            });
        }

        private void HandleError(Exception ex)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
            Message = "Error";
        }

        private void UpdateMessage(string message)
        {
            Rhino.RhinoApp.InvokeOnUiThread((Action)delegate
            {
                Message = message;
            });
        }
    }
}