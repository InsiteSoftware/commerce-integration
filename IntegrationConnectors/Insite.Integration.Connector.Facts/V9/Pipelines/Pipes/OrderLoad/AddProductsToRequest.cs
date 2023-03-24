namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class AddProductsToRequest : IPipe<OrderLoadParameter, OrderLoadResult>
{
    public int Order => 600;

    public OrderLoadResult Execute(
        IUnitOfWork unitOfWork,
        OrderLoadParameter parameter,
        OrderLoadResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddProductsToRequest)} Started.");

        foreach (var orderLine in parameter.CustomerOrder.OrderLines)
        {
            result.OrderLoadRequest.Request.Orders
                .First()
                .OrderDetail.Add(
                    new LineItemInfo
                    {
                        SequenceNumber = orderLine.Line,
                        ItemNumber = orderLine.Product?.ErpNumber ?? string.Empty,
                        LineItemType = string.Empty,
                        ShipInstructionType = false,
                        OrderQty = orderLine.QtyOrdered,
                        UnitOfMeasure = orderLine.UnitOfMeasure,
                        WarehouseID = orderLine.Warehouse?.Name,
                        ActualSellPrice = orderLine.UnitNetPrice
                    }
                );
        }

        parameter.JobLogger?.Debug($"{nameof(AddProductsToRequest)} Finished.");

        return result;
    }
}
