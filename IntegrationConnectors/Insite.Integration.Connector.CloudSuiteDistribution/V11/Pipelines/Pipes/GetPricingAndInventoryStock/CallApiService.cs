namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetPricingAndInventoryStock;

using Insite.Common.Dependencies;
using Insite.Common.Logging;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.OePricingMultipleV4;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

public sealed class CallApiService
    : IPipe<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
{
    private readonly ICloudSuiteDistributionClient cloudSuiteDistributionClient;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.cloudSuiteDistributionClient =
            dependencyLocator.GetInstance<ICloudSuiteDistributionClient>();
    }

    public int Order => 300;

    public GetPricingAndInventoryStockResult Execute(
        IUnitOfWork unitOfWork,
        GetPricingAndInventoryStockParameter parameter,
        GetPricingAndInventoryStockResult result
    )
    {
        this.LogRedactingCredentials(result.OePricingMultipleV4Request); // "log, redacting credentials"

        var getPricingAndInventoryStockResult =
            this.cloudSuiteDistributionClient.GetPricingAndInventoryStock(
                parameter.IntegrationConnection,
                result.SerializedOePricingMultipleV4Request
            );
        if (getPricingAndInventoryStockResult.resultCode != ResultCode.Success)
        {
            return PipelineHelper.CreateErrorPipelineResult(
                result,
                SubCode.GeneralFailure,
                getPricingAndInventoryStockResult.response
            );
        }

        result.SerializedOePricingMultipleV4Response = getPricingAndInventoryStockResult.response;

        LogHelper
            .For(this)
            .Debug(
                $"{nameof(OePricingMultipleV4Response)}: {result.SerializedOePricingMultipleV4Response}"
            );

        return result;
    }

    /// <summary>
    /// Redact the OperatorInit and OperatorPassword fields so their real values don't end up
    /// in the debugging output, do the output, and then restore the real values
    /// </summary>
    private void LogRedactingCredentials(OePricingMultipleV4Request requestWrapper)
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
            .Debug($"{nameof(OePricingMultipleV4Request)}: {serializedRequestWrapper}");

        requestWrapper.Request.OperatorPassword = pass;
        requestWrapper.Request.OperatorInit = user;
    }
}
