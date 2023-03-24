namespace Insite.Integration.Connector.Facts.Services;

internal static class FactsCloneService
{
    public static T Clone<T>(T source)
    {
        var serialized = FactsSerializationService.Serialize(source);

        return FactsSerializationService.Deserialize<T>(serialized);
    }
}
