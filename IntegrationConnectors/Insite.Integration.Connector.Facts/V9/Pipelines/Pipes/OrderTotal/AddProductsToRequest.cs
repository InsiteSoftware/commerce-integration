namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderTotal;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderTotal;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class AddProductsToRequest : IPipe<OrderTotalParameter, OrderTotalResult>
{
    public int Order => 500;

    public OrderTotalResult Execute(
        IUnitOfWork unitOfWork,
        OrderTotalParameter parameter,
        OrderTotalResult result
    )
    {
        foreach (var orderLine in parameter.CustomerOrder.OrderLines)
        {
            result.OrderTotalRequest.Request.Orders
                .First()
                .OrderDetail.Add(
                    new LineItemInfo
                    {
                        SequenceNumber = orderLine.Line,
                        ItemNumber = orderLine.Product.ErpNumber,
                        OrderQty = orderLine.QtyOrdered,
                        UnitOfMeasure = orderLine.UnitOfMeasure,
                        WarehouseID = orderLine.Warehouse.Name,
                        ActualSellPrice = orderLine.UnitNetPrice
                    }
                );
        }

        return result;
    }
}
