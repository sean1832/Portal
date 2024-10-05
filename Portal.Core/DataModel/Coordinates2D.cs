using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public class Coordinates2DConverter: JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            dynamic coordinates = value;
            if (coordinates != null) {
                writer.WriteStartArray();
                writer.WriteValue(coordinates.X);
                writer.WriteValue(coordinates.Y);
                writer.WriteEndArray();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Ensure we're reading an array
            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new JsonSerializationException($"Unexpected token parsing Coordinates2D. Expected StartArray, got {reader.TokenType}.");
            }

            if (objectType.BaseType == null)
            {
                throw new JsonSerializationException($"Unexpected token parsing Coordinates2D. BaseType is null.");
            }

            reader.Read(); // Move to the first element in the array

            // Get the type parameter T (float or double)
            Type tType = objectType.BaseType.GetGenericArguments()[0];

            // Deserialize each coordinate as the correct type
            object x = serializer.Deserialize(reader, tType);
            reader.Read(); // Move to the next element
            object y = serializer.Deserialize(reader, tType);
            reader.Read(); // Move to the EndArray token

            if (reader.TokenType != JsonToken.EndArray)
            {
                throw new JsonSerializationException($"Unexpected token parsing Coordinates2D. Expected EndArray, got {reader.TokenType}.");
            }

            // Create an instance of the object using the correct constructor
            return Activator.CreateInstance(objectType, x, y);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsSubclassOf(typeof(Coordinates2D<>));
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;
    }

    [JsonConverter(typeof(Coordinates2DConverter))]
    public abstract class Coordinates2D<T>
    {
        public T X { get; set; }
        public T Y { get; set; }

        protected Coordinates2D() { }

        protected Coordinates2D(T x, T y)
        {
            X = x;
            Y = y;
        }
    }

    public class PVector2Df:Coordinates2D<float>
    {
        public PVector2Df(float x, float y) : base(x, y)
        {
        }
    }

    public class PVector2Di : Coordinates2D<int>
    {
        public PVector2Di(int x, int y) : base(x, y)
        {
        }

        public PVector2Di(Size size) : base(size.Width, size.Height)
        {
        }
    }
}
