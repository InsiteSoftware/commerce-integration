namespace Insite.Integration.Connector.Facts.Services;

using Insite.Data.Entities;

internal interface IFactsApiServiceFactory
{
    IFactsApiService GetFactsApiService(IntegrationConnection integrationConnection = null);
}
