using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using Portal.Core.Binary;
using Portal.Gh.Common;
using Portal.Gh.Params.Bytes;

namespace Portal.Gh.Components.Local
{
    public class FileWriterComponent : GH_Component
    {
        #region Metadata

        public FileWriterComponent()
            : base("File Writer", ">File<",
                "Write a file in bytes",
                Config.Category, Config.SubCat.Local)
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.tertiary;
        public override IEnumerable<string> Keywords => new string[] { };
        protected override Bitmap Icon => Icons.FileWrite;
        public override Guid ComponentGuid => new Guid("cdbb0c22-a0b5-43bd-a937-7bf7549c45e7");

        #endregion

        #region IO

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new BytesParam(), "Bytes", "Bytes", "bytes to write", GH_ParamAccess.item);
            pManager.AddTextParameter("File Path", "path", "file path to write", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write", "write", "start writing", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        #endregion

        private ushort _lastChecksum;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BytesGoo data = null;
            string path = string.Empty;
            bool write = false;

            if (!DA.GetData(0, ref data)) return;
            if (!DA.GetData(1, ref path)) return;
            if (!DA.GetData(2, ref write)) return;

            if (!write)
            {
                _lastChecksum = 0; // Reset checksum
                return;
            }
            ushort checksum = Packet.Deserialize(data.Value).Header.Checksum;
            if (_lastChecksum != 0 && _lastChecksum  == checksum) return; // // Skip if the message is the same

            // if parent directory does not exist, create it
            string dir = System.IO.Path.GetDirectoryName(path);
            if (dir != null && !System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }

            System.IO.File.WriteAllBytes(path, Packet.Deserialize(data.Value).Data);
            _lastChecksum = checksum;
            Message = "Written";
        }
    }
}