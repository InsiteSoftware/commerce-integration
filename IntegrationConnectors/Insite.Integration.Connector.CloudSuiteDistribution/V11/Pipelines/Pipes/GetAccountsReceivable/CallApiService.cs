namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetAccountsReceivable;

using Insite.Common.Dependencies;
using Insite.Common.Logging;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.SfCustomerSummary;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

public sealed class CallApiService
    : IPipe<GetAccountsReceivableParameter, GetAccountsReceivableResult>
{
    private readonly ICloudSuiteDistributionClient cloudSuiteDistributionClient;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.cloudSuiteDistributionClient =
            dependencyLocator.GetInstance<ICloudSuiteDistributionClient>();
    }

    public int Order => 300;

    public GetAccountsReceivableResult Execute(
        IUnitOfWork unitOfWork,
        GetAccountsReceivableParameter parameter,
        GetAccountsReceivableResult result
    )
    {
        this.LogRedactingCredentials(result.SfCustomerSummaryRequest); // "log, redacting credentials"

        var getAccountsReceivableResult = this.cloudSuiteDistributionClient.GetAccountsReceivable(
            parameter.IntegrationConnection,
            result.SerializedSfCustomerSummaryRequest
        );
        if (getAccountsReceivableResult.resultCode != ResultCode.Success)
        {
            return PipelineHelper.CreateErrorPipelineResult(
                result,
                SubCode.GeneralFailure,
                getAccountsReceivableResult.response
            );
        }

        result.SerializedSfCustomerSummaryResponse = getAccountsReceivableResult.response;

        LogHelper
            .For(this)
            .Debug(
                $"{nameof(SfCustomerSummaryResponse)}: {result.SerializedSfCustomerSummaryResponse}"
            );

        return result;
    }

    /// <summary>
    /// Redact the OperatorInit and OperatorPassword fields so their real values don't end up
    /// in the debugging output, do the output, and then restore the real values
    /// </summary>
    private void LogRedactingCredentials(SfCustomerSummaryRequest requestWrapper)
    {
        if (requestWrapper == null || requestWrapper.Request == null)
        {
            return;
        }

        var pass = requestWrapper.Request.OperatorPassword;
        var user = requestWrapper.Request.OperatorInit;

        requestWrapper.Request.OperatorPassword = "[redacted]";
        requestWrapper.Request.OperatorInit = "[redacted]";

        var serializedRequestWrapper = CloudSuiteDistributionSerializationService.Serialize(
            requestWrapper
        );

        LogHelper
            .For(this)
            .Debug($"{nameof(SfCustomerSummaryRequest)}: {serializedRequestWrapper}");

        requestWrapper.Request.OperatorPassword = pass;
        requestWrapper.Request.OperatorInit = user;
    }
}
