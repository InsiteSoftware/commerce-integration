namespace Insite.Integration.Connector.Acumatica.Services;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Enums;
using Insite.RealTimeInventory.Services;
using Insite.RealTimeInventory.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.Acumatica))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.Acumatica)]
public sealed class ExternalInventoryServiceAcumatica : ExternalInventoryServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public ExternalInventoryServiceAcumatica(
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
        var inventoryAllocationInquiryParameter = new InventoryAllocationInquiryParameter
        {
            GetInventoryParameter = productInventoryParameter,
            IntegrationConnection = this.GetIntegrationConnection()
        };

        var inventoryAllocationInquiryResult = this.pipeAssemblyFactory.ExecutePipeline(
            inventoryAllocationInquiryParameter,
            new InventoryAllocationInquiryResult()
        );

        return inventoryAllocationInquiryResult.GetInventoryResult;
    }
}
