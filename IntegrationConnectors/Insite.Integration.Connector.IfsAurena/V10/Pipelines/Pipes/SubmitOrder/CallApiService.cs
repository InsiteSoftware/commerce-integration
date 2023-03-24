namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.Services;

public sealed class CallApiService : IPipe<SubmitOrderParameter, SubmitOrderResult>
{
    private readonly IIfsAurenaClient ifsAurenaClient;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.ifsAurenaClient = dependencyLocator.GetInstance<IIfsAurenaClient>();
    }

    public int Order => 300;

    public SubmitOrderResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderParameter parameter,
        SubmitOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Started.");
        parameter.JobLogger?.Debug(
            $"{nameof(CustomerOrder)} Request: {result.SerializedCustomerOrderRequest}"
        );

        var submitOrderResult = this.ifsAurenaClient.SubmitOrder(
            parameter.IntegrationConnection,
            result.SerializedCustomerOrderRequest
        );
        if (submitOrderResult.resultCode != ResultCode.Success)
        {
            parameter.JobLogger?.Error(
                $"{nameof(CallApiService)} Failed: {submitOrderResult.response}"
            );

            return PipelineHelper.CreateErrorPipelineResult(
                result,
                SubCode.GeneralFailure,
                submitOrderResult.response
            );
        }

        result.SerializedCustomerOrderResponse = submitOrderResult.response;

        parameter.JobLogger?.Debug(
            $"{nameof(CustomerOrder)} Response: {result.SerializedCustomerOrderResponse}"
        );
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Finished.");

        return result;
    }
}
