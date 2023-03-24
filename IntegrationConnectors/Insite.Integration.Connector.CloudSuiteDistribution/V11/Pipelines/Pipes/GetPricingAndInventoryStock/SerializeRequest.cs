namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetPricingAndInventoryStock;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

public sealed class SerializeRequest
    : IPipe<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
{
    public int Order => 200;

    public GetPricingAndInventoryStockResult Execute(
        IUnitOfWork unitOfWork,
        GetPricingAndInventoryStockParameter parameter,
        GetPricingAndInventoryStockResult result
    )
    {
        result.SerializedOePricingMultipleV4Request =
            CloudSuiteDistributionSerializationService.Serialize(result.OePricingMultipleV4Request);

        return result;
    }
}
