namespace Insite.Integration.Connector.APlus.V10.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;

public class CreateOrderResult : PipeResultBase
{
    public CreateOrderRequest CreateOrderRequest { get; set; }

    public CreateOrderResponse CreateOrderResponse { get; set; }

    public string ErpOrderNumber { get; set; }

    public decimal TaxAmount { get; set; }

    public bool TaxCalculated { get; set; }
}
