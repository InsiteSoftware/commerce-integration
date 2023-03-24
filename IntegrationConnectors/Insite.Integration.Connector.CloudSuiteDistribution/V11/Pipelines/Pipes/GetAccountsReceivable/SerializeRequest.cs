namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetAccountsReceivable;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

public sealed class SerializeRequest
    : IPipe<GetAccountsReceivableParameter, GetAccountsReceivableResult>
{
    public int Order => 200;

    public GetAccountsReceivableResult Execute(
        IUnitOfWork unitOfWork,
        GetAccountsReceivableParameter parameter,
        GetAccountsReceivableResult result
    )
    {
        result.SerializedSfCustomerSummaryRequest =
            CloudSuiteDistributionSerializationService.Serialize(result.SfCustomerSummaryRequest);

        return result;
    }
}
