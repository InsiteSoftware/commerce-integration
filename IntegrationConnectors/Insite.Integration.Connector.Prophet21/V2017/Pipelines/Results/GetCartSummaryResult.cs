namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetCartSummary;

public class GetCartSummaryResult : PipeResultBase
{
    public GetCartSummary GetCartSummaryRequest { get; set; }

    public GetCartSummary GetCartSummaryReply { get; set; }

    public decimal TaxAmount { get; set; }

    public bool TaxCalculated { get; set; }
}
