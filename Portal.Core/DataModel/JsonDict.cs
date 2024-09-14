using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Portal.Core.DataModel
{
    public class JsonDict : Dictionary<string, object>, IDisposable
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
