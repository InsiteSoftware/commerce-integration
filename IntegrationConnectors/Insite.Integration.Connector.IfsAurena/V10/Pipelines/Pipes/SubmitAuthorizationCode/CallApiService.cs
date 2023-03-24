namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitAuthorizationCode;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class CallApiService
    : IPipe<SubmitAuthorizationCodeParameter, SubmitAuthorizationCodeResult>
{
    private readonly IIfsAurenaClient ifsAurenaClient;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.ifsAurenaClient = dependencyLocator.GetInstance<IIfsAurenaClient>();
    }

    public int Order => 300;

    public SubmitAuthorizationCodeResult Execute(
        IUnitOfWork unitOfWork,
        SubmitAuthorizationCodeParameter parameter,
        SubmitAuthorizationCodeResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Started.");
        parameter.JobLogger?.Debug(
            $"{nameof(CreditCardDetails)} Request: {result.SerializedAuthorizationCodeRequest}"
        );

        var submitOrderResult = this.ifsAurenaClient.SubmitAuthorizationCode(
            parameter.IntegrationConnection,
            result.SerializedAuthorizationCodeRequest
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
