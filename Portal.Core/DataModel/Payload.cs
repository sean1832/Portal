using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Portal.Core.DataModel
{
    public class Payload
    {
        public object Items { get; set; }
        public JsonDict Meta { get; set; }

        public Payload() { }

        public Payload(object items)
        {
            Items = items;
        }
        public Payload(object items, JsonDict meta)
        {
            Items = items;
            Meta = meta;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
