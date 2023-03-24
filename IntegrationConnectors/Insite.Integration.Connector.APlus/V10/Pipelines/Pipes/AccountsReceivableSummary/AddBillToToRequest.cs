namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.AccountsReceivableSummary;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class AddBillToToRequest
    : IPipe<AccountsReceivableSummaryParameter, AccountsReceivableSummaryResult>
{
    public int Order => 200;

    public AccountsReceivableSummaryResult Execute(
        IUnitOfWork unitOfWork,
        AccountsReceivableSummaryParameter parameter,
        AccountsReceivableSummaryResult result
    )
    {
        if (parameter.GetAgingBucketsParameter.Customer == null)
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages.Add(new ResultMessage { Message = "BillTo customer is required." });
            return result;
        }

        if (string.IsNullOrEmpty(parameter.GetAgingBucketsParameter.Customer.ErpNumber))
        {
            // customers not yet submitted to erp
            result.GetAgingBucketsResult = new GetAgingBucketsResult();
            result.ExitPipeline = true;
            return result;
        }

        result.AccountsReceivableSummaryRequest.CustomerNumber = parameter
            .GetAgingBucketsParameter
            .Customer
            .ErpNumber;

        return result;
    }
}
