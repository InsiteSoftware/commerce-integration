namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetAccountsReceivable;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

public sealed class ValidateResponse
    : IPipe<GetAccountsReceivableParameter, GetAccountsReceivableResult>
{
    public int Order => 500;

    public GetAccountsReceivableResult Execute(
        IUnitOfWork unitOfWork,
        GetAccountsReceivableParameter parameter,
        GetAccountsReceivableResult result
    )
    {
        if (string.IsNullOrEmpty(result.SfCustomerSummaryResponse?.Response?.ErrorMessage))
        {
            return result;
        }

        return PipelineHelper.CreateErrorPipelineResult(
            result,
            SubCode.GeneralFailure,
            result.SfCustomerSummaryResponse.Response.ErrorMessage
        );
    }
}
