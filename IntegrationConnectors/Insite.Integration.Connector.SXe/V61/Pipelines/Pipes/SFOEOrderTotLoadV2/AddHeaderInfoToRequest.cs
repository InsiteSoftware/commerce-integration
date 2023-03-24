namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System;
using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

public sealed class AddHeaderInfoToRequest
    : IPipe<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    public int Order => 400;

    public SFOEOrderTotLoadV2Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV2Parameter parameter,
        SFOEOrderTotLoadV2Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Started.");

        result.SFOEOrderTotLoadV2Request.arrayInheader = GetInInHeaders(parameter);

        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Finished.");

        return result;
    }

    private static List<SFOEOrderTotLoadV2inputInheader> GetInInHeaders(
        SFOEOrderTotLoadV2Parameter parameter
    )
    {
        var inInHeader1 = new SFOEOrderTotLoadV2inputInheader
        {
            carrierCode = parameter.CustomerOrder.ShipVia?.ErpShipCode ?? string.Empty,
            orderType = GetOrderType(parameter.IsOrderSubmit),
            poNumber = parameter.CustomerOrder.CustomerPO,
            requestedShipDate = GetRequestedShipDate(parameter.CustomerOrder),
            taxAmount = parameter.CustomerOrder.TaxAmount,
            warehouseID = parameter.CustomerOrder.DefaultWarehouse?.Name ?? string.Empty,
            webTransactionType = GetWebTransactionType(parameter.IsOrderSubmit)
        };

        return new List<SFOEOrderTotLoadV2inputInheader> { inInHeader1 };
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
