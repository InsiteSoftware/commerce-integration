namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderTotal;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class GetTaxAmountFromResponse : IPipe<OrderTotalParameter, OrderTotalResult>
{
    public int Order => 700;

    public OrderTotalResult Execute(
        IUnitOfWork unitOfWork,
        OrderTotalParameter parameter,
        OrderTotalResult result
    )
    {
        if (result.OrderTotalResponse?.Response?.Orders?.Any() ?? false)
        {
            result.TaxAmount = result.OrderTotalResponse.Response.Orders.First().SalesTaxAmount;
            result.TaxCalculated = true;
        }

        return result;
    }
}
