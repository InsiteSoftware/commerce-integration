namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class CreateInitialRequest : IPipe<GetCartSummaryParameter, GetCartSummaryResult>
{
    public int Order => 100;

    public GetCartSummaryResult Execute(
        IUnitOfWork unitOfWork,
        GetCartSummaryParameter parameter,
        GetCartSummaryResult result
    )
    {
        result.GetCartSummaryRequest = new GetCartSummary
        {
            Request = new Request
            {
                B2BSellerVersion = new RequestB2BSellerVersion
                {
                    MajorVersion = "5",
                    MinorVersion = "11",
                    BuildNumber = "100"
                }
            }
        };

        return result;
    }
}
