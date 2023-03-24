namespace Insite.Integration.Connector.Prophet21.Services;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.RealTimeInventory.Services;
using Insite.RealTimeInventory.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.Prophet21))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.Prophet21)]
public sealed class ExternalInventoryServiceProphet21 : ExternalInventoryServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    private readonly IProphet21IntegrationConnectionProvider prophet21IntegrationConnectionProvider;

    public ExternalInventoryServiceProphet21(
        IUnitOfWorkFactory unitOfWorkFactory,
        RealTimeInventorySettings realTimeInventorySettings,
        IPipeAssemblyFactory pipeAssemblyFactory,
        IDependencyLocator dependencyLocator
    )
        : base(unitOfWorkFactory, realTimeInventorySettings)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
        this.prophet21IntegrationConnectionProvider =
            dependencyLocator.GetInstance<IProphet21IntegrationConnectionProvider>();
    }

    public override GetInventoryResult GetInventory(GetInventoryParameter productInventoryParameter)
    {
        var integrationConnection =
            this.prophet21IntegrationConnectionProvider.GetIntegrationConnection(
                this.GetIntegrationConnection()
            );

        var getItemPriceParameter = new GetItemPriceParameter
        {
            GetInventoryParameter = productInventoryParameter,
            IntegrationConnection = integrationConnection
        };

        var getItemPriceResult = this.pipeAssemblyFactory.ExecutePipeline(
            getItemPriceParameter,
            new GetItemPriceResult()
        );

        return getItemPriceResult.GetInventoryResult;
    }
}
