namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.AccountsReceivableSummary;

using System;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.APlus.Services;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class CallApiService
    : IPipe<AccountsReceivableSummaryParameter, AccountsReceivableSummaryResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 400;

    public AccountsReceivableSummaryResult Execute(
        IUnitOfWork unitOfWork,
        AccountsReceivableSummaryParameter parameter,
        AccountsReceivableSummaryResult result
    )
    {
        try
        {
            result.AccountsReceivableSummaryResponse = this.dependencyLocator
                .GetInstance<IAPlusApiServiceFactory>()
                .GetAPlusApiService(parameter.IntegrationConnection)
                .AccountsReceivableSummary(result.AccountsReceivableSummaryRequest);
        }
        catch (Exception exception)
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages.Add(new ResultMessage { Message = exception.Message });
        }

        return result;
    }
}
