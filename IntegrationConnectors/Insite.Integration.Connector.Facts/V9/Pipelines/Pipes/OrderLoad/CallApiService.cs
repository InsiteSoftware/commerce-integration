namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;

using System;
using System.Linq;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Facts.Services;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class CallApiService : IPipe<OrderLoadParameter, OrderLoadResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 800;

    public OrderLoadResult Execute(
        IUnitOfWork unitOfWork,
        OrderLoadParameter parameter,
        OrderLoadResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Started.");
        parameter.JobLogger?.Debug(
            $"{nameof(OrderLoadRequest)}: {FactsSerializationService.Serialize(result.OrderLoadRequest)}"
        );

        try
        {
            result.OrderLoadResponse = this.dependencyLocator
                .GetInstance<IFactsApiServiceFactory>()
                .GetFactsApiService(parameter.IntegrationConnection)
                .OrderLoad(result.OrderLoadRequest);
        }
        catch (Exception exception)
        {
            return PipelineHelper.CreateErrorPipelineResult(
                result,
                SubCode.GeneralFailure,
                exception.Message
            );
        }

        if (
            result.OrderLoadResponse.Response.Orders.First().CompletionCode.EqualsIgnoreCase("E")
            || !string.IsNullOrEmpty(result.OrderLoadResponse.Response.Orders.First().Message)
        )
        {
            var errorMessage =
                $"Error submitting customer order: "
                + $"{nameof(OrderLoadRequest)}: {FactsSerializationService.Serialize(result.OrderLoadRequest)} "
                + $"{nameof(OrderLoadResponse)}: {FactsSerializationService.Serialize(result.OrderLoadResponse)}";

            return PipelineHelper.CreateErrorPipelineResult(
                result,
                SubCode.GeneralFailure,
                errorMessage
            );
        }

        parameter.JobLogger?.Debug(
            $"{nameof(OrderLoadResponse)}: {FactsSerializationService.Serialize(result.OrderLoadResponse)}"
        );
        parameter.JobLogger?.Debug($"{nameof(CallApiService)} Finished.");

        return result;
    }
}
