namespace Insite.Integration.Connector.Ifs.Services;

using Insite.Data.Entities;

internal interface IIfsApiServiceFactory
{
    /// <summary>Gets the IIfsApiService.</summary>
    /// <returns>The <see cref="IIfsApiService"/>.</returns>
    /// <param name="integrationConnection">The integration connection used to initialize the <see cref="IIfsApiService"/>.</param>
    IIfsApiService GetIfsApiService(IntegrationConnection integrationConnection = null);
}
