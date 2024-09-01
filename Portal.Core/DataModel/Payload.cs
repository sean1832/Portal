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
        public object Data { get; set; }
        public JsonDict Metadata { get; set; }

        public Payload() { }

        public Payload(object data)
        {
            Data = data;
        }
        public Payload(object data, JsonDict metadata)
        {
            Data = data;
            Metadata = metadata;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
