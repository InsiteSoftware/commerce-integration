namespace Insite.Integration.Connector.SXe.Services;

using Insite.Core.Interfaces.Dependency;

internal interface IIntegrationConnectorServiceSXeFactory : ISingletonLifetime
{
    IIntegrationConnectorServiceSXe GetIntegrationConnectorServiceSXe();
}
