namespace Insite.Integration.Connector.SXe.Services;

using Insite.Common.Dependencies;
using Insite.Core.SystemSetting.Groups.Integration;

internal sealed class IntegrationConnectorServiceSXeFactory : IIntegrationConnectorServiceSXeFactory
{
    private readonly IDependencyLocator dependencyLocator;

    public IntegrationConnectorServiceSXeFactory(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public IIntegrationConnectorServiceSXe GetIntegrationConnectorServiceSXe()
    {
        var sXeVersion = this.dependencyLocator
            .GetInstance<IntegrationConnectorSettings>()
            .SXeVersion.ToString();

        return this.dependencyLocator.GetInstance<IIntegrationConnectorServiceSXe>(sXeVersion);
    }
}
