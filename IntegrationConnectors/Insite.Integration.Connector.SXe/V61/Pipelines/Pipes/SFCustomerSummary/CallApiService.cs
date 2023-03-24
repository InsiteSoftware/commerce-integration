namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFCustomerSummary;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;

public sealed class CallApiService : IPipe<SFCustomerSummaryParameter, SFCustomerSummaryResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 300;

    public SFCustomerSummaryResult Execute(
        IUnitOfWork unitOfWork,
        SFCustomerSummaryParameter parameter,
        SFCustomerSummaryResult result
    )
    {
        result.SFCustomerSummaryResponse = this.dependencyLocator
            .GetInstance<ISXeApiServiceFactory>()
            .GetSXeApiServiceV61(parameter.IntegrationConnection)
            .SFCustomerSummary(result.SFCustomerSummaryRequest);

        if (!string.IsNullOrEmpty(result.SFCustomerSummaryResponse.errorMessage))
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages.Add(
                new ResultMessage { Message = result.SFCustomerSummaryResponse.errorMessage }
            );
        }

        return result;
    }
}
