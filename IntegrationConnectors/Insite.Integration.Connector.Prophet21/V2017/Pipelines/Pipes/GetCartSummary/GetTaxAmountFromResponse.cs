namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class GetTaxAmountFromResponse : IPipe<GetCartSummaryParameter, GetCartSummaryResult>
{
    public int Order => 800;

    public GetCartSummaryResult Execute(
        IUnitOfWork unitOfWork,
        GetCartSummaryParameter parameter,
        GetCartSummaryResult result
    )
    {
        if (
            decimal.TryParse(
                result.GetCartSummaryReply?.Reply?.CartSummary?.SalesTax,
                out var salesTaxAmount
            )
        )
        {
            result.TaxCalculated = true;
        }

        result.TaxAmount = salesTaxAmount;

        return result;
    }
}
