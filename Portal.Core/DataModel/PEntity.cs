using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public abstract class PEntity
    {
        public PType Type { get; }

        protected PEntity(PType type)
        {
            Type = type;
        }
    }
}
