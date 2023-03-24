namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;

public sealed class AddHeaderInfoToRequest
    : IPipe<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    public int Order => 400;

    public SFOEOrderTotLoadV4Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV4Parameter parameter,
        SFOEOrderTotLoadV4Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Started.");

        result.SFOEOrderTotLoadV4Request.Ininheader = GetInInHeaders(parameter);

        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Finished.");

        return result;
    }

    private static List<Ininheader3> GetInInHeaders(SFOEOrderTotLoadV4Parameter parameter)
    {
        var inInHeader3 = new Ininheader3
        {
            CarrierCode = parameter.CustomerOrder.ShipVia?.ErpShipCode ?? string.Empty,
            OrderType = GetOrderType(parameter.IsOrderSubmit),
            PoNumber = parameter.CustomerOrder.CustomerPO,
            RequestedShipDate = GetRequestedShipDate(parameter.CustomerOrder),
            TaxAmount = parameter.CustomerOrder.TaxAmount,
            WarehouseID = parameter.CustomerOrder.DefaultWarehouse?.Name ?? string.Empty,
            WebTransactionType = GetWebTransactionType(parameter.IsOrderSubmit)
        };

        return new List<Ininheader3> { inInHeader3 };
    }

    private static string GetOrderType(bool isOrderSubmit)
    {
        return isOrderSubmit ? "O" : string.Empty;
    }

    private static string GetRequestedShipDate(CustomerOrder customerOrder)
    {
        var requestedShipDate = customerOrder.FulfillmentMethod.EqualsIgnoreCase(
            FulfillmentMethod.PickUp.ToString()
        )
            ? customerOrder.RequestedPickupDate
            : customerOrder.RequestedDeliveryDate;

        return requestedShipDate?.ToString("yyyy/MM/dd") ?? string.Empty;
    }

    private static string GetWebTransactionType(bool isOrderSubmit)
    {
        return isOrderSubmit ? "LSF" : "TSF";
    }
}
