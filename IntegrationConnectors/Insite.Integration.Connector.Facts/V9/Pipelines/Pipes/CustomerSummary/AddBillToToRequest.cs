namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.CustomerSummary;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class AddBillToToRequest : IPipe<CustomerSummaryParameter, CustomerSummaryResult>
{
    public int Order => 200;

    public CustomerSummaryResult Execute(
        IUnitOfWork unitOfWork,
        CustomerSummaryParameter parameter,
        CustomerSummaryResult result
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

        result.CustomerSummaryRequest.Request.Customer = parameter
            .GetAgingBucketsParameter
            .Customer
            .ErpNumber;

        return result;
    }
}
