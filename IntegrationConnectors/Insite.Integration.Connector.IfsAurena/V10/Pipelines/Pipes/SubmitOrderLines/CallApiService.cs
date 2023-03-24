namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrderLines;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.Services;

public sealed class CallApiService : IPipe<SubmitOrderLinesParameter, SubmitOrderLinesResult>
{
    private readonly IIfsAurenaClient ifsAurenaClient;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.ifsAurenaClient = dependencyLocator.GetInstance<IIfsAurenaClient>();
    }

    public int Order => 300;

    public SubmitOrderLinesResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderLinesParameter parameter,
        SubmitOrderLinesResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Started.");

        var serializedOrderLineResponses = new ConcurrentBag<string>();
        try
        {
            foreach (var serializedOrderLineRequest in result.SerializedCustomerOrderLineRequests)
            {
                parameter.JobLogger?.Debug(
                    $"{nameof(CustomerOrderLine)} Request: {serializedOrderLineRequest}"
                );

                var submitOrderLineResult = this.ifsAurenaClient.SubmitOrderLine(
                    parameter.IntegrationConnection,
                    parameter.ErpOrderNumber,
                    serializedOrderLineRequest
                );
                if (submitOrderLineResult.resultCode != ResultCode.Success)
                {
                    throw new Exception(submitOrderLineResult.response);
                }

                serializedOrderLineResponses.Add(submitOrderLineResult.response);

                parameter.JobLogger?.Debug(
                    $"{nameof(CustomerOrderLine)} Response: {submitOrderLineResult.response}"
                );
            }

            result.SerializedCustomerOrderLineResponses = serializedOrderLineResponses.ToList();
        }
        catch (Exception ex)
        {
            var errorMessage = ex.ToString();

            parameter.JobLogger?.Error($"{nameof(CallApiService)} Failed: {errorMessage}");

            return PipelineHelper.CreateErrorPipelineResult(
                result,
                SubCode.GeneralFailure,
                errorMessage
            );
        }

        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Finished.");

        return result;
    }
}
