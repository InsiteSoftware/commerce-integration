namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.CustomerSummary;

using System;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Facts.Services;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class CallApiService : IPipe<CustomerSummaryParameter, CustomerSummaryResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 400;

    public CustomerSummaryResult Execute(
        IUnitOfWork unitOfWork,
        CustomerSummaryParameter parameter,
        CustomerSummaryResult result
    )
    {
        try
        {
            result.CustomerSummaryResponse = this.dependencyLocator
                .GetInstance<IFactsApiServiceFactory>()
                .GetFactsApiService(parameter.IntegrationConnection)
                .CustomerSummary(result.CustomerSummaryRequest);
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
