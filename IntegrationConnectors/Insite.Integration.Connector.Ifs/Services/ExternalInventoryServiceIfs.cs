namespace Insite.Integration.Connector.Ifs.Services;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.RealTimeInventory.Services;
using Insite.RealTimeInventory.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.IFS))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.IFS)]
public sealed class ExternalInventoryServiceIfs : ExternalInventoryServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public ExternalInventoryServiceIfs(
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
        var getPartAvailabilityParameter = new GetPartAvailabilityParameter
        {
            GetInventoryParameter = productInventoryParameter,
            IntegrationConnection = this.GetIntegrationConnection()
        };

        var getPartAvailabilityResult = this.pipeAssemblyFactory.ExecutePipeline(
            getPartAvailabilityParameter,
            new GetPartAvailabilityResult()
        );

        return getPartAvailabilityResult.GetInventoryResult;
    }
}
