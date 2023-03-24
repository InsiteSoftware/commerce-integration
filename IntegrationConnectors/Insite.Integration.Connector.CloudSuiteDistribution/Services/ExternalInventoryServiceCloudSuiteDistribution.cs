namespace Insite.Integration.Connector.CloudSuiteDistribution.Services;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using Insite.RealTimeInventory.Services;
using Insite.RealTimeInventory.SystemSettings;

[DependencyName(nameof(IntegrationConnectorType.CloudSuiteDistribution))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.CloudSuiteDistribution)]
public sealed class ExternalInventoryServiceCloudSuiteDistribution : ExternalInventoryServiceBase
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    private readonly ICloudSuiteDistributionIntegrationConnectionProvider cloudSuiteDistributionIntegrationConnectionProvider;

    public ExternalInventoryServiceCloudSuiteDistribution(
        IUnitOfWorkFactory unitOfWorkFactory,
        RealTimeInventorySettings realTimeInventorySettings,
        IPipeAssemblyFactory pipeAssemblyFactory,
        IDependencyLocator dependencyLocator
    )
        : base(unitOfWorkFactory, realTimeInventorySettings)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
        this.cloudSuiteDistributionIntegrationConnectionProvider =
            dependencyLocator.GetInstance<ICloudSuiteDistributionIntegrationConnectionProvider>();
    }

    public override GetInventoryResult GetInventory(GetInventoryParameter productInventoryParameter)
    {
        var integrationConnection =
            this.cloudSuiteDistributionIntegrationConnectionProvider.GetIntegrationConnection(
                this.GetIntegrationConnection()
            );

        var getPricingAndInventoryStockParameter = new GetPricingAndInventoryStockParameter
        {
            GetInventoryParameter = productInventoryParameter,
            IntegrationConnection = integrationConnection
        };

        var getPricingAndInventoryStockResult = this.pipeAssemblyFactory.ExecutePipeline(
            getPricingAndInventoryStockParameter,
            new GetPricingAndInventoryStockResult()
        );

        return getPricingAndInventoryStockResult.GetInventoryResult;
    }
}
