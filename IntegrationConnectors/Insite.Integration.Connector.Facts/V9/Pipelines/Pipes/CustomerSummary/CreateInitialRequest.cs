namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.CustomerSummary;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.CustomerSummary;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class CreateInitialRequest : IPipe<CustomerSummaryParameter, CustomerSummaryResult>
{
    public int Order => 100;

    public CustomerSummaryResult Execute(
        IUnitOfWork unitOfWork,
        CustomerSummaryParameter parameter,
        CustomerSummaryResult result
    )
    {
        result.CustomerSummaryRequest = new CustomerSummaryRequest
        {
            Request = new Request { RequestID = "CustomerSummary" }
        };

        return result;
    }
}
