namespace Insite.WIS.InRiver.Services.FileServiceTypes.Azure.Factories;

using Insite.WIS.InRiver.Services.FileServiceTypes.Azure.Providers;

public class AzureProviderFactory : IAzureProviderFactory
{
    public IAzureProvider CreateAzureProvider(
        string azureStorageConnectionString,
        string containerName
    )
    {
        return new AzureProvider(azureStorageConnectionString, containerName);
    }
}
