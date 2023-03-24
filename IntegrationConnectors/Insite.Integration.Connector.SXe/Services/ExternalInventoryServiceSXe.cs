namespace Insite.Integration.Connector.SXe.Services;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Inventory;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.RealTimeInventory.Services;
using Insite.RealTimeInventory.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.SXe))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.SXe)]
public sealed class ExternalInventoryServiceSXe : ExternalInventoryServiceBase
{
    private readonly IDependencyLocator dependencyLocator;

    public ExternalInventoryServiceSXe(
        IUnitOfWorkFactory unitOfWorkFactory,
        RealTimeInventorySettings realTimeInventorySettings,
        IDependencyLocator dependencyLocator
    )
        : base(unitOfWorkFactory, realTimeInventorySettings)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public override GetInventoryResult GetInventory(GetInventoryParameter productInventoryParameter)
    {
        return this.dependencyLocator
            .GetInstance<IIntegrationConnectorServiceSXeFactory>()
            .GetIntegrationConnectorServiceSXe()
            .GetInventory(productInventoryParameter, this.GetIntegrationConnection());
    }
}
