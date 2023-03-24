namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFCustomerSummary;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFCustomerSummary;

public sealed class CreateInitialRequest
    : IPipe<SFCustomerSummaryParameter, SFCustomerSummaryResult>
{
    public int Order => 100;

    public SFCustomerSummaryResult Execute(
        IUnitOfWork unitOfWork,
        SFCustomerSummaryParameter parameter,
        SFCustomerSummaryResult result
    )
    {
        result.SFCustomerSummaryRequest = new SFCustomerSummaryRequest();

        return result;
    }
}
