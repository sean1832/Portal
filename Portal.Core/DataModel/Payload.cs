using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public class Payload
    {
        public object Data { get; set; }
        public JsonDict Metadata { get; set; }
    }
}
