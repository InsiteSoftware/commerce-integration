namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrderCharges;

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

public sealed class CallApiService : IPipe<SubmitOrderChargesParameter, SubmitOrderChargesResult>
{
    private readonly IIfsAurenaClient ifsAurenaClient;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.ifsAurenaClient = dependencyLocator.GetInstance<IIfsAurenaClient>();
    }

    public int Order => 300;

    public SubmitOrderChargesResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderChargesParameter parameter,
        SubmitOrderChargesResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Started.");

        var tasks = new List<Task>();
        var serializedCustomerOrderChargeResponses = new ConcurrentBag<string>();

        foreach (
            var serializedCustomerOrderChargeRequest in result.SerializedCustomerOrderChargeRequests
        )
        {
            var task = Task.Factory.StartNew(() =>
            {
                parameter.JobLogger?.Debug(
                    $"{nameof(CustomerOrderCharge)} Request: {serializedCustomerOrderChargeRequest}"
                );

                var submitOrderChargeResult = this.ifsAurenaClient.SubmitOrderCharge(
                    parameter.IntegrationConnection,
                    parameter.ErpOrderNumber,
                    serializedCustomerOrderChargeRequest
                );
                if (submitOrderChargeResult.resultCode != ResultCode.Success)
                {
                    // will aggregate in AggregateException handling in main thread below
                    throw new Exception(submitOrderChargeResult.response);
                }

                serializedCustomerOrderChargeResponses.Add(submitOrderChargeResult.response);

                parameter.JobLogger?.Debug(
                    $"{nameof(CustomerOrderCharge)} Response: {submitOrderChargeResult.response}"
                );
            });

            tasks.Add(task);
        }

        try
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            Task.WaitAll(tasks.ToArray());
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            result.SerializedCustomerOrderChargeResponses =
                serializedCustomerOrderChargeResponses.ToList();
        }
        catch (AggregateException aggregateException)
        {
            var errorMessage = string.Join(
                Environment.NewLine,
                aggregateException.InnerExceptions.Select(o => o.ToString())
            );

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
