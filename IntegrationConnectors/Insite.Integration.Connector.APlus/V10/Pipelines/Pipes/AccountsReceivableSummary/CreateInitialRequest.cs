namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.AccountsReceivableSummary;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.AccountsReceivableSummary;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class CreateInitialRequest
    : IPipe<AccountsReceivableSummaryParameter, AccountsReceivableSummaryResult>
{
    public int Order => 100;

    public AccountsReceivableSummaryResult Execute(
        IUnitOfWork unitOfWork,
        AccountsReceivableSummaryParameter parameter,
        AccountsReceivableSummaryResult result
    )
    {
        result.AccountsReceivableSummaryRequest = new AccountsReceivableSummaryRequest
        {
            Name = "ARSummary"
        };

        return result;
    }
}
