namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFOEOrderTotLoadV4;

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

        result.SFOEOrderTotLoadV4Request.Request.InputHeaderDataCollection =
            new InputHeaderDataCollection { InputHeaderDatas = GetInInHeaders(parameter) };

        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Finished.");

        return result;
    }

    private static List<InputHeaderData> GetInInHeaders(SFOEOrderTotLoadV4Parameter parameter)
    {
        var inputHeaderData = new InputHeaderData
        {
            Carriercode = parameter.CustomerOrder.ShipVia?.ErpShipCode ?? string.Empty,
            Ordertype = GetOrderType(parameter.IsOrderSubmit),
            Ponumber = parameter.CustomerOrder.CustomerPO,
            Reqshipdate = GetRequestedShipDate(parameter.CustomerOrder),
            Taxamount = parameter.CustomerOrder.TaxAmount,
            Warehouseid = parameter.CustomerOrder.DefaultWarehouse?.Name ?? string.Empty,
            Webtransactiontype = GetWebTransactionType(parameter.IsOrderSubmit)
        };

        return new List<InputHeaderData> { inputHeaderData };
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
