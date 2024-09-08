using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public abstract class PEntity
    {
        public PGeoType GeoGeoType { get; }

        protected PEntity(PGeoType geoGeoType)
        {
            GeoGeoType = geoGeoType;
        }
    }
}
