using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public abstract class PEntity
    {
        public PGeoType Type { get; }

        protected PEntity(PGeoType type)
        {
            Type = type;
        }
    }
}
