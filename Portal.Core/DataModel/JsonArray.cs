using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Portal.Core.DataModel
{
    public class JsonArray: List<object>, IDisposable
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public string ToString(bool indented)
        {
            return JsonConvert.SerializeObject(this);
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
