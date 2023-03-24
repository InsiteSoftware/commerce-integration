namespace Insite.Integration.Connector.APlus.Services;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;
using Insite.RealTimeInventory.Services;
using Insite.RealTimeInventory.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.APlus))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.APlus)]
public sealed class ExternalInventoryServiceAPlus : ExternalInventoryServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public ExternalInventoryServiceAPlus(
        IUnitOfWorkFactory unitOfWorkFactory,
        RealTimeInventorySettings realTimeInventorySettings,
        IPipeAssemblyFactory pipeAssemblyFactory
    )
        : base(unitOfWorkFactory, realTimeInventorySettings)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public override GetInventoryResult GetInventory(GetInventoryParameter productInventoryParameter)
    {
        var getLineItemPriceAndAvailabilityParameter = new LineItemPriceAndAvailabilityParameter()
        {
            GetInventoryParameter = productInventoryParameter,
            IntegrationConnection = this.GetIntegrationConnection()
        };

        var getLineItemPriceAndAvailabilityResult = this.pipeAssemblyFactory.ExecutePipeline(
            getLineItemPriceAndAvailabilityParameter,
            new LineItemPriceAndAvailabilityResult()
        );

        return getLineItemPriceAndAvailabilityResult.GetInventoryResult;
    }
}
