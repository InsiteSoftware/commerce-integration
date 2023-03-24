namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetMyAccountOpenAR;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddBillToToRequest
    : IPipe<GetMyAccountOpenARParameter, GetMyAccountOpenARResult>
{
    public int Order => 200;

    public GetMyAccountOpenARResult Execute(
        IUnitOfWork unitOfWork,
        GetMyAccountOpenARParameter parameter,
        GetMyAccountOpenARResult result
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

        result.GetMyAccountOpenARRequest.Request.CustomerCode = parameter
            .GetAgingBucketsParameter
            .Customer
            .ErpNumber;

        return result;
    }
}
