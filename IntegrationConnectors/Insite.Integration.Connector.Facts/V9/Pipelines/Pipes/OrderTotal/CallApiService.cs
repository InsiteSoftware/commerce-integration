namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderTotal;

using System;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Facts.Services;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class CallApiService : IPipe<OrderTotalParameter, OrderTotalResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 600;

    public OrderTotalResult Execute(
        IUnitOfWork unitOfWork,
        OrderTotalParameter parameter,
        OrderTotalResult result
    )
    {
        try
        {
            result.OrderTotalResponse = this.dependencyLocator
                .GetInstance<IFactsApiServiceFactory>()
                .GetFactsApiService(parameter.IntegrationConnection)
                .OrderTotal(result.OrderTotalRequest);
        }
        catch (Exception exception)
        {
            return PipelineHelper.CreateErrorPipelineResult(
                result,
                SubCode.GeneralFailure,
                exception.Message
            );
        }

        return result;
    }
}
