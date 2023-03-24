namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFCustomerSummary;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;

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
            .GetSXeApiServiceV10(parameter.IntegrationConnection)
            .SFCustomerSummary(result.SFCustomerSummaryRequest);

        if (!string.IsNullOrEmpty(result.SFCustomerSummaryResponse.ErrorMessage))
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages.Add(
                new ResultMessage { Message = result.SFCustomerSummaryResponse.ErrorMessage }
            );
        }

        return result;
    }
}
