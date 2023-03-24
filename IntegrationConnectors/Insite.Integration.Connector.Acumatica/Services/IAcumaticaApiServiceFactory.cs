namespace Insite.Integration.Connector.Acumatica.Services;

using Insite.Data.Entities;

internal interface IAcumaticaApiServiceFactory
{
    IAcumaticaApiService GetAcumaticaApiService(IntegrationConnection integrationConnection = null);
}
