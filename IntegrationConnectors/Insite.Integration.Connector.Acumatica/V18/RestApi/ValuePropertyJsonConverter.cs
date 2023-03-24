namespace Insite.Integration.Connector.Acumatica.V18.RestApi;

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal class ValuePropertyJsonConverter : JsonConverter
{
    private const string ValuePropertyName = "value";

    public override bool CanConvert(Type objectType)
    {
        // this converter should only be used with the JsonConverter attribute
        return false;
    }

    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer
    )
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        return JObject
                .Load(reader)
                .Properties()
                .FirstOrDefault(
                    o => o.Name.Equals(ValuePropertyName, StringComparison.OrdinalIgnoreCase)
                )
                ?.Value?.ToObject(objectType, serializer) ?? GetDefault(objectType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName(ValuePropertyName);
        serializer.Serialize(writer, value);
        writer.WriteEndObject();
    }

    private static object GetDefault(Type objectType)
    {
        return objectType.IsValueType ? Activator.CreateInstance(objectType) : null;
    }
}
