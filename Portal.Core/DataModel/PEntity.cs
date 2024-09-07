using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public abstract class PEntity
    {
        public string Type { get; }

        protected PEntity(string type)
        {
            Type = type;
        }
    }
}
