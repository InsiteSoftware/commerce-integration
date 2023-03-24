namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetPricing;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Insite.Common.Dependencies;
using Insite.Common.Logging;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class CallApiService : IPipe<GetPricingParameter, GetPricingResult>
{
    private readonly IIfsAurenaClient ifsAurenaClient;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.ifsAurenaClient = dependencyLocator.GetInstance<IIfsAurenaClient>();
    }

    public int Order => 300;

    public GetPricingResult Execute(
        IUnitOfWork unitOfWork,
        GetPricingParameter parameter,
        GetPricingResult result
    )
    {
        var tasks = new List<Task>();
        var serializedPriceQueryResponses = new ConcurrentBag<string>();

        foreach (var serializedPriceQueryRequest in result.SerializedPriceQueryRequests)
        {
            var task = Task.Factory.StartNew(() =>
            {
                LogHelper
                    .For(this)
                    .Debug($"{nameof(PriceQuery)} Request: {serializedPriceQueryRequest}");

                var getPricingResult = this.ifsAurenaClient.GetPricing(
                    parameter.IntegrationConnection,
                    serializedPriceQueryRequest
                );
                if (getPricingResult.resultCode != ResultCode.Success)
                {
                    // will aggregate in AggregateException handling in main thread below
                    throw new Exception(getPricingResult.response);
                }

                serializedPriceQueryResponses.Add(getPricingResult.response);

                LogHelper
                    .For(this)
                    .Debug($"{nameof(PriceQuery)} Response: {getPricingResult.response}");
            });

            tasks.Add(task);
        }

        try
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            Task.WaitAll(tasks.ToArray());
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            result.SerializedPriceQueryResponses = serializedPriceQueryResponses.ToList();
        }
        catch (AggregateException aggregateException)
        {
            var errorMessage = string.Join(
                Environment.NewLine,
                aggregateException.InnerExceptions.Select(o => o.ToString())
            );
            return PipelineHelper.CreateErrorPipelineResult(
                result,
                SubCode.RealTimePricingGeneralFailure,
                errorMessage
            );
        }

        // clear price queries from IFS
        this.ifsAurenaClient.CleanPriceQuery(parameter.IntegrationConnection);

        return result;
    }
}
