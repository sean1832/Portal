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
    internal class PLightConverterSettings : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("CanWrite is false");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            Int64 typeInt = (obj["PLightType"] ?? throw new InvalidOperationException(@"Type is not defined")).Value<Int64>();
            PGeoType type = (PGeoType)typeInt;
            switch (type)
            {
                case PGeoType.PointLight:
                    return obj.ToObject<PPointLight>(serializer);
                case PGeoType.RectangularLight:
                    return obj.ToObject<PRectangularLight>(serializer);
                case PGeoType.SpotLight:
                    return obj.ToObject<PSpotLight>(serializer);
                default:
                    throw new NotImplementedException($"Deserialization of {type} is not supported");
            }
        }   

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PLight);
        }

        public override bool CanWrite => false;
    }
}
