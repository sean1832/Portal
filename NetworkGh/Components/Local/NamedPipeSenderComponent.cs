using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using NetworkGh.Common;
using NetworkGh.Core.NamedPipe;
using System.Threading.Tasks;
using NetworkGh.Core.Utils;

namespace NetworkGh.Components.Local
{
    public class NamedPipeSenderComponent : GH_Component
    {
        #region Metadata

        public NamedPipeSenderComponent()
            : base("Named Pipe Sender", "<pipe>",
                "Client that sends messages to a named pipe server. " +
                "\n\nNamed Pipes:\n" +
                "Provides reliable inter-process communication within the same machine, using stream-based data transfer. " +
                "Named Pipes are highly reliable and suitable for complex data exchanges within a single local machine, " +
                "but they do not support remote communication."
                , Config.Category, Config.SubCat.LocalIpc)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override IEnumerable<string> Keywords => new string[] { "np sender", "np client", "pipe sender" };
        protected override Bitmap Icon => Icons.PiperSender;
        public override Guid ComponentGuid => new Guid("a90b6d3b-f595-4bab-9f79-5536c9c4bf67");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Server Name", "Server", "The name or IP address of the server hosting the named pipe. Use '.' for the local machine.", GH_ParamAccess.item, ".");
            pManager.AddTextParameter("Pipe Name", "Pipe", "The unique identifier for the named pipe. This name is used by both the server and clients to connect.", GH_ParamAccess.item);
            pManager.AddTextParameter("Message", "msg", "The message to be sent to the server via the named pipe.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Send", "Send", "Set to true to send the message to the server.", GH_ParamAccess.item,
                false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        #endregion

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string serverName = null;
            string pipeName = null;
            string message = null;
            bool send = false;

            if (!DA.GetData(0, ref serverName)) return;
            if (!DA.GetData(1, ref pipeName)) return;
            if (!DA.GetData(2, ref message)) return;
            if (!DA.GetData(3, ref send)) return;
            

            if (!send) return;

            Task.Run(() =>
            {
                try
                {
                    using (var client = new NamedPipeClient(serverName, pipeName, HandleError))
                    {
                        client.Connect();
                        client.SendAsync(System.Text.Encoding.UTF8.GetBytes(message)).Wait();
                        UpdateMessage($@"\\{serverName}\pipe\{pipeName}");
                    }
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