namespace Insite.Integration.Connector.IfsAurena.Services;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Insite.RealTimeInventory.Services;
using Insite.RealTimeInventory.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.IFSAurena))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.IFSAurena)]
public sealed class ExternalInventoryServiceIfsAurena : ExternalInventoryServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public ExternalInventoryServiceIfsAurena(
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
        var getInventoryStockParameter = new GetInventoryStockParameter
        {
            GetInventoryParameter = productInventoryParameter,
            IntegrationConnection = this.GetIntegrationConnection()
        };

        var getInventoryStockResult = this.pipeAssemblyFactory.ExecutePipeline(
            getInventoryStockParameter,
            new GetInventoryStockResult()
        );

        return getInventoryStockResult.GetInventoryResult;
    }
}
