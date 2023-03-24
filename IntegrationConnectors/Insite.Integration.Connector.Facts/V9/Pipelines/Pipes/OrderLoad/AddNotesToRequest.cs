namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class AddNotesToRequest : IPipe<OrderLoadParameter, OrderLoadResult>
{
    public int Order => 700;

    public OrderLoadResult Execute(
        IUnitOfWork unitOfWork,
        OrderLoadParameter parameter,
        OrderLoadResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddNotesToRequest)} Started.");

        foreach (
            var orderLine in parameter.CustomerOrder.OrderLines.Where(
                o => !string.IsNullOrEmpty(o.Notes)
            )
        )
        {
            result.OrderLoadRequest.Request.Orders
                .First()
                .OrderDetail.Add(
                    new LineItemInfo
                    {
                        SequenceNumber = orderLine.Line,
                        ItemNumber = string.Empty,
                        LineItemType = "&amp;N",
                        ItemDescription1 = orderLine.Notes,
                        ItemDescription2 = string.Empty,
                        ShipInstructionType = false,
                        OrderQty = 0,
                        UnitOfMeasure = string.Empty,
                        WarehouseID = string.Empty,
                        ActualSellPrice = 0
                    }
                );
        }

        if (!string.IsNullOrEmpty(parameter.CustomerOrder.Notes))
        {
            result.OrderLoadRequest.Request.Orders
                .First()
                .OrderDetail.Add(
                    new LineItemInfo
                    {
                        SequenceNumber = parameter.CustomerOrder.OrderLines.Count + 1,
                        ItemNumber = string.Empty,
                        LineItemType = "/N",
                        ItemDescription1 = parameter.CustomerOrder.Notes,
                        ItemDescription2 = string.Empty,
                        ShipInstructionType = false,
                        OrderQty = 0,
                        UnitOfMeasure = string.Empty,
                        WarehouseID = string.Empty,
                        ActualSellPrice = 0
                    }
                );
        }

        parameter.JobLogger?.Debug($"{nameof(AddNotesToRequest)} Finished.");

        return result;
    }
}
