namespace Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;

using System;
using System.Linq;
using Insite.Common.Providers;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

public sealed class AddHeaderInfoToRequest : IPipe<OrderLoadParameter, OrderLoadResult>
{
    private const string RequestedShipDateFormat = "yyyy-MM-dd";

    public int Order => 200;

    public OrderLoadResult Execute(
        IUnitOfWork unitOfWork,
        OrderLoadParameter parameter,
        OrderLoadResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Started.");

        var orderHeader = result.OrderLoadRequest.Request.Orders.First().OrderHeader;

        orderHeader.WebTransactionType = "LSF";
        orderHeader.OrderType = "O";
        orderHeader.WebOrderID = parameter.CustomerOrder.OrderNumber;
        orderHeader.WarehouseID = parameter.CustomerOrder.DefaultWarehouse?.Name;
        orderHeader.CarrierCode = parameter.CustomerOrder.ShipVia?.ErpShipCode;
        orderHeader.PoNumber = parameter.CustomerOrder.CustomerPO;
        orderHeader.ReviewOrderHold = string.Empty; // Blank = normal, “S” = Service Hold, “C” = credit hold
        orderHeader.IsIncludeFreight = true;
        orderHeader.FreightAmount = parameter.CustomerOrder.ShippingCharges;
        orderHeader.HandlingAmount = parameter.CustomerOrder.HandlingCharges;

        if (
            parameter.CustomerOrder.FulfillmentMethod.EqualsIgnoreCase(
                FulfillmentMethod.PickUp.ToString()
            )
        )
        {
            orderHeader.RequestedShipDate =
                parameter.CustomerOrder.RequestedPickupDate?.ToString(RequestedShipDateFormat)
                ?? DateTimeProvider.Current.Now.ToString(RequestedShipDateFormat);
            orderHeader.IsPickUpOrder = true;
        }
        else
        {
            orderHeader.RequestedShipDate =
                parameter.CustomerOrder.RequestedDeliveryDate?.ToString(RequestedShipDateFormat)
                ?? parameter.CustomerOrder.RequestedShipDate?.ToString(RequestedShipDateFormat)
                ?? DateTimeProvider.Current.Now.ToString(RequestedShipDateFormat);
            orderHeader.IsPickUpOrder = false;
        }

        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Finished.");

        return result;
    }
}
