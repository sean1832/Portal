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
            Int64 typeInt = (obj["PCurveType"] ?? throw new InvalidOperationException(@"Type is not defined")).Value<Int64>();
            PCurveType type = (PCurveType)typeInt;
            switch (type)
            {
                case PCurveType.Nurbs:
                    return obj.ToObject<PNurbsCurve>(serializer);
                case PCurveType.Polyline:
                    return obj.ToObject<PPolylineCurve>(serializer);
                case PCurveType.Line:
                    return obj.ToObject<PLine>(serializer);
                case PCurveType.Arc:
                    return obj.ToObject<PArcCurve>(serializer);
                case PCurveType.Base:
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
