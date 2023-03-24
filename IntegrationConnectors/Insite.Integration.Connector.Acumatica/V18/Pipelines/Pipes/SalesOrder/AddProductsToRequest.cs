namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;

public sealed class AddProductsToRequest : IPipe<SalesOrderParameter, SalesOrderResult>
{
    public int Order => 700;

    public SalesOrderResult Execute(
        IUnitOfWork unitOfWork,
        SalesOrderParameter parameter,
        SalesOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddProductsToRequest)} Started.");

        result.SalesOrderRequest.Details = GetDetails(parameter.CustomerOrder);

        parameter.JobLogger?.Debug($"{nameof(AddProductsToRequest)} Finished.");

        return result;
    }

    private static List<Detail> GetDetails(CustomerOrder customerOrder)
    {
        var details = new List<Detail>();

        foreach (var orderLine in customerOrder.OrderLines.OrderBy(o => o.Line))
        {
            details.Add(
                new Detail
                {
                    rowNumber = orderLine.Line,
                    InventoryID = orderLine.Product?.ErpNumber ?? string.Empty,
                    OrderQty = orderLine.QtyOrdered,
                    UOM = orderLine.UnitOfMeasure,
                    TaxCategory = orderLine.TaxCode1,
                    UnitPrice = orderLine.UnitNetPrice,
                    ExtendedPrice = orderLine.UnitNetPrice * orderLine.QtyOrdered,
                    OvershipThreshold = 100,
                    UndershipThreshold = 100
                }
            );
        }

        return details;
    }
}
