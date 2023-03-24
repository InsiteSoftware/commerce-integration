namespace Insite.Integration.Connector.APlus.Services;

using Insite.Data.Entities;

internal interface IAPlusApiServiceFactory
{
    /// <summary>Gets the IAPlusApiService.</summary>
    /// <returns>The <see cref="IAPlusApiService"/>.</returns>
    /// <param name="integrationConnection">The integration connection used to initialize the <see cref="IAPlusApiService"/>.</param>
    IAPlusApiService GetAPlusApiService(IntegrationConnection integrationConnection = null);
}
