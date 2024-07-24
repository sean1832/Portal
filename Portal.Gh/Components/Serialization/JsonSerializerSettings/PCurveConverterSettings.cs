using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Portal.Core.DataModel;

namespace Portal.Gh.Components.Serialization.JsonSerializerSettings
{
    internal class PCurveConverterSettings : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("CanWrite is false");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            var type = (obj["Type"] ?? throw new InvalidOperationException(@"Type is not defined")).Value<string>();
            switch (type)
            {
                case nameof(PNurbsCurve):
                    return obj.ToObject<PNurbsCurve>(serializer);
                case nameof(PPolylineCurve):
                    return obj.ToObject<PPolylineCurve>(serializer);
                case nameof(PLine):
                    return obj.ToObject<PLine>(serializer);
                case nameof(PArcCurve):
                    return obj.ToObject<PArcCurve>(serializer);
                case nameof(PCurve):
                    return obj.ToObject<PCurve>(serializer);
                default:
                    throw new NotImplementedException($"Deserialization of {type} is not supported");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PCurve);
        }

        public override bool CanWrite => false;
    }
}
