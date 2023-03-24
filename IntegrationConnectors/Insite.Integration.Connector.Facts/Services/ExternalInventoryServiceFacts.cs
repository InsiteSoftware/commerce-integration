namespace Insite.Integration.Connector.Facts.Services;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.RealTimeInventory.Services;
using Insite.RealTimeInventory.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.FACTS))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.FACTS)]
public sealed class ExternalInventoryServiceFacts : ExternalInventoryServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public ExternalInventoryServiceFacts(
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
        var priceAvailabilityParameter = new PriceAvailabilityParameter()
        {
            GetInventoryParameter = productInventoryParameter,
            IntegrationConnection = this.GetIntegrationConnection()
        };

        var priceAvailabilityResult = this.pipeAssemblyFactory.ExecutePipeline(
            priceAvailabilityParameter,
            new PriceAvailabilityResult()
        );

        return priceAvailabilityResult.GetInventoryResult;
    }
}
