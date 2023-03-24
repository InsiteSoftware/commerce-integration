namespace Insite.Integration.Connector.IfsAurena.Services;

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

internal static class IfsAurenaSerializationService
{
    private static readonly JsonSerializer JsonSerializer = new JsonSerializer
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

    public static string Serialize<T>(T value)
    {
        return JsonConvert.SerializeObject(value, JsonSerializerSettings);
    }

    public static T Deserialize<T>(string value)
    {
        return typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>)
            ? JObject.Parse(value)["value"].ToObject<T>(JsonSerializer)
            : JsonConvert.DeserializeObject<T>(value, JsonSerializerSettings);
    }
}
