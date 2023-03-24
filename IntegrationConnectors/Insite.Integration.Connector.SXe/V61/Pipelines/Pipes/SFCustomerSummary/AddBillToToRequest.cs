namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFCustomerSummary;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;

public sealed class AddBillToToRequest : IPipe<SFCustomerSummaryParameter, SFCustomerSummaryResult>
{
    public int Order => 200;

    public SFCustomerSummaryResult Execute(
        IUnitOfWork unitOfWork,
        SFCustomerSummaryParameter parameter,
        SFCustomerSummaryResult result
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

        if (
            !decimal.TryParse(
                parameter.GetAgingBucketsParameter.Customer.ErpNumber,
                out var customerErpNumber
            )
        )
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages.Add(
                new ResultMessage { Message = "Customer ErpNumber must be in decimal format." }
            );
            return result;
        }

        result.SFCustomerSummaryRequest.customerNumber = customerErpNumber;

        return result;
    }
}
