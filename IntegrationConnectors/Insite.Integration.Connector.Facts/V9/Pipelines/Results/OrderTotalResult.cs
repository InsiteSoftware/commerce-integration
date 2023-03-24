namespace Insite.Integration.Connector.Facts.V9.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderTotal;

public class OrderTotalResult : PipeResultBase
{
    public OrderTotalRequest OrderTotalRequest { get; set; }

    public OrderTotalResponse OrderTotalResponse { get; set; }

    public decimal TaxAmount { get; set; }

    public bool TaxCalculated { get; set; }
}
