// reference JSwan by andrew heumann:
// https://github.com/andrewheumann/jSwan/blob/master/jSwan/Param_JsonInput.cs
using Grasshopper.Kernel;
using Portal.Gh.Common;
using Portal.Gh.Params.Bytes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel.Parameters;
using GH_IO.Serialization;
using System.Windows.Forms;

namespace Portal.Gh.Params.Json
{
    public class JsonDictGenericParam : Param_GenericObject, IDisposable
    {
        private void Menu_ItemAccessClicked(object sender, EventArgs e)
        {
            if (Access != 0)
            {
                Access = GH_ParamAccess.item;
                OnObjectChanged(GH_ObjectEventType.DataMapping);
                ExpireSolution(recompute: true);
            }
        }

        private void Menu_ListAccessClicked(object sender, EventArgs e)
        {
            if (Access != GH_ParamAccess.list)
            {
                Access = GH_ParamAccess.list;
                OnObjectChanged(GH_ObjectEventType.DataMapping);
                ExpireSolution(recompute: true);
            }
        }

        //currently unused, leaving in just in case
        private void Menu_TreeAccessClicked(object sender, EventArgs e)
        {
            if (Access != GH_ParamAccess.tree)
            {
                Access = GH_ParamAccess.tree;
                OnObjectChanged(GH_ObjectEventType.DataMapping);
                ExpireSolution(recompute: true);
            }
        }


        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            if (Kind != GH_ParamKind.output)
            {

                Menu_AppendItem(menu, "Item Access", Menu_ItemAccessClicked, true, Access == GH_ParamAccess.item);
                Menu_AppendItem(menu, "List Access", Menu_ListAccessClicked, true, Access == GH_ParamAccess.list);

            }
        }

        protected override Bitmap Icon => Icons.JsonParam;

        public override Guid ComponentGuid => new Guid("c0c23e50-53d9-4aad-abbf-ad53f149cbec");

        public override bool Write(GH_IWriter writer)
        {
            var result = base.Write(writer);
            writer.SetInt32("ScriptParamAccess", (int)Access);
            return result;
        }


        public override bool Read(GH_IReader reader)
        {
            var result = base.Read(reader);

            if (reader.ItemExists("ScriptParamAccess"))
            {
                try
                {
                    Access = (GH_ParamAccess)reader.GetInt32("ScriptParamAccess");
                    return result;
                }
                catch (Exception ex)
                {
                    //smoosh
                }
            }
            return result;
        }

        public void Dispose()
        {
            this.ClearData();
        }
    }
}
