namespace Insite.Integration.Connector.SXe.V10.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;

public class SFOEOrderTotLoadV4Result : PipeResultBase
{
    public SFOEOrderTotLoadV4Request SFOEOrderTotLoadV4Request { get; set; }

    public SFOEOrderTotLoadV4Response SFOEOrderTotLoadV4Response { get; set; }

    public string ErpOrderNumber { get; set; }

    public decimal TaxAmount { get; set; }

    public bool TaxCalculated { get; set; }
}
