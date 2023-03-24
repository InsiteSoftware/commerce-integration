namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetAccountsReceivable;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.SfCustomerSummary;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

public sealed class DeserializeResponse
    : IPipe<GetAccountsReceivableParameter, GetAccountsReceivableResult>
{
    public int Order => 400;

    public GetAccountsReceivableResult Execute(
        IUnitOfWork unitOfWork,
        GetAccountsReceivableParameter parameter,
        GetAccountsReceivableResult result
    )
    {
        result.SfCustomerSummaryResponse =
            CloudSuiteDistributionSerializationService.Deserialize<SfCustomerSummaryResponse>(
                result.SerializedSfCustomerSummaryResponse
            );

        return result;
    }
}
