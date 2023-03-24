namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;

using System;
using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class AddProductsToRequest
    : IPipe<CreateCustomerOrderParameter, CreateCustomerOrderResult>
{
    public int Order => 600;

    public CreateCustomerOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateCustomerOrderParameter parameter,
        CreateCustomerOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddProductsToRequest)} Started.");

        result.CustomerOrder.lines = GetCustomerOrderLines(parameter.CustomerOrder);

        parameter.JobLogger?.Debug($"{nameof(AddProductsToRequest)} Finished.");

        return result;
    }

    private static List<customerOrderLine> GetCustomerOrderLines(CustomerOrder customerOrder)
    {
        var customerOrderLines = new List<customerOrderLine>();

        foreach (var orderLine in customerOrder.OrderLines)
        {
            customerOrderLines.Add(
                new customerOrderLine
                {
                    lineNo = orderLine.Line.ToString(),
                    catalogNo = orderLine.Product?.ErpNumber ?? string.Empty,
                    buyQtyDue = orderLine.QtyOrdered,
                    buyQtyDueSpecified = true,
                    convFactor = GetConversionFactor(orderLine),
                    convFactorSpecified = true,
                    saleUnitPrice = orderLine.UnitRegularPrice,
                    saleUnitPriceSpecified = true,
                    noteText = GetNoteText(orderLine)
                }
            );
        }

        return customerOrderLines;
    }

    private static decimal GetConversionFactor(OrderLine orderLine)
    {
        var productUnitOfMeasure = orderLine.Product.ProductUnitOfMeasures.FirstOrDefault(
            o => o.UnitOfMeasure.Equals(orderLine.UnitOfMeasure, StringComparison.OrdinalIgnoreCase)
        );

        return productUnitOfMeasure?.QtyPerBaseUnitOfMeasure ?? 1;
    }

    private static string GetNoteText(OrderLine orderLine)
    {
        return orderLine.Notes.Length <= 2000
            ? orderLine.Notes
            : orderLine.Notes.Substring(0, 2000);
    }
}
