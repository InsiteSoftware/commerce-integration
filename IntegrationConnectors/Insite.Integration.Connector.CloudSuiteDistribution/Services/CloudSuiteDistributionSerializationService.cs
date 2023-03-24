namespace Insite.Integration.Connector.CloudSuiteDistribution.Services;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

internal static class CloudSuiteDistributionSerializationService
{
    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore
        };

    public static string Serialize<T>(T value)
    {
        return JsonConvert.SerializeObject(value, JsonSerializerSettings);
    }

    public static T Deserialize<T>(string value)
    {
        return JsonConvert.DeserializeObject<T>(value, JsonSerializerSettings);
    }
}
