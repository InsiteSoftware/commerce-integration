namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderTotal;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class AddHeaderInfoToRequest : IPipe<OrderTotalParameter, OrderTotalResult>
{
    public int Order => 200;

    public OrderTotalResult Execute(
        IUnitOfWork unitOfWork,
        OrderTotalParameter parameter,
        OrderTotalResult result
    )
    {
        var orderHeader = result.OrderTotalRequest.Request.Orders.First().OrderHeader;

        orderHeader.WebTransactionType = "TSF";
        orderHeader.WarehouseID = parameter.CustomerOrder.DefaultWarehouse.Name;
        orderHeader.IsPickUpOrder = parameter.CustomerOrder.FulfillmentMethod.EqualsIgnoreCase(
            FulfillmentMethod.PickUp.ToString()
        );
        orderHeader.RequestedShipDate = string.Empty; // required to be in the request

        return result;
    }
}
