using System;
using Mcma.Core;
using Newtonsoft.Json;

namespace Mcma.Json
{
    public class TypeConverter : JsonConverter<Type>
    {
        public override void WriteJson(JsonWriter writer, Type value, JsonSerializer serializer)
            => writer.WriteValue(value.Name);

        public override Type ReadJson(JsonReader reader, Type objectType, Type existingValue, bool hasExistingValue, JsonSerializer serializer)
            => reader.Value?.ToString()?.ToResourceType();
    }
}