namespace Insite.WIS.InRiver.Services.FileServiceTypes.Azure.Factories;

using Insite.WIS.InRiver.Services.FileServiceTypes.Azure.Providers;

public interface IAzureProviderFactory
{
    IAzureProvider CreateAzureProvider(string azureStorageConnectionString, string containerName);
}
