using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LogParserConcept.Json
{
    public class IgnoreInteger64ZeroConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value != null && (Int64)value > 0)
            {
                serializer.Serialize(writer, value);
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize<int>(reader);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Int64);
        }
    }
}
