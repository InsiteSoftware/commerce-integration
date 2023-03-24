namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddProductsToRequest : IPipe<GetCartSummaryParameter, GetCartSummaryResult>
{
    public int Order => 600;

    public GetCartSummaryResult Execute(
        IUnitOfWork unitOfWork,
        GetCartSummaryParameter parameter,
        GetCartSummaryResult result
    )
    {
        foreach (var orderLine in parameter.CustomerOrder.OrderLines)
        {
            result.GetCartSummaryRequest.Request.ListOfLineItems.Add(
                new RequestLineItem
                {
                    ItemID = orderLine.Product.ErpNumber,
                    OrderQuantity = orderLine.QtyOrdered.ToString(),
                    UnitName = orderLine.UnitOfMeasure,
                    SourceLocation = orderLine.Warehouse?.Name,
                    CalculateTax = "TRUE"
                }
            );
        }

        return result;
    }
}
