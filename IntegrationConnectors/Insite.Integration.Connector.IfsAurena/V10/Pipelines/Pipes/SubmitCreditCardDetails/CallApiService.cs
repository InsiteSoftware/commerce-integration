namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitCreditCardDetails;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class CallApiService
    : IPipe<SubmitCreditCardDetailsParameter, SubmitCreditCardDetailsResult>
{
    private readonly IIfsAurenaClient ifsAurenaClient;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.ifsAurenaClient = dependencyLocator.GetInstance<IIfsAurenaClient>();
    }

    public int Order => 300;

    public SubmitCreditCardDetailsResult Execute(
        IUnitOfWork unitOfWork,
        SubmitCreditCardDetailsParameter parameter,
        SubmitCreditCardDetailsResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Started.");
        parameter.JobLogger?.Debug(
            $"{nameof(CreditCardDetails)} Request: {result.SerializedCreditCardDetailsRequest}"
        );

        var submitOrderResult = this.ifsAurenaClient.SubmitCreditCardDetails(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            result.SerializedCreditCardDetailsRequest
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

        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Finished.");

        return result;
    }
}
