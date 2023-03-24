namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddProductsToRequest : IPipe<OrderImportParameter, OrderImportResult>
{
    public int Order => 800;

    public OrderImportResult Execute(
        IUnitOfWork unitOfWork,
        OrderImportParameter parameter,
        OrderImportResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddProductsToRequest)} Started.");

        foreach (var orderLine in parameter.CustomerOrder.OrderLines)
        {
            result.OrderImportRequest.Request.ListOfLineItems.Add(
                new RequestLineItem
                {
                    ItemID = orderLine.Product.ErpNumber,
                    OrderQuantity = orderLine.QtyOrdered.ToString(),
                    UnitName = orderLine.UnitOfMeasure,
                    UnitPrice = orderLine.UnitNetPrice.ToString(),
                    SourceLocation = orderLine.Warehouse?.Name,
                    NotepadText = orderLine.Notes
                }
            );
        }

        parameter.JobLogger?.Debug($"{nameof(AddProductsToRequest)} Finished.");

        return result;
    }
}
