namespace Insite.Integration.Connector.SXe.V61.Pipelines.Results;

using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

public class SFOEOrderTotLoadV2Result : PipeResultBase
{
    public SFOEOrderTotLoadV2Request SFOEOrderTotLoadV2Request { get; set; }

    public SFOEOrderTotLoadV2Response SFOEOrderTotLoadV2Response { get; set; }

    public string ErpOrderNumber { get; set; }

    public decimal TaxAmount { get; set; }

    public bool TaxCalculated { get; set; }
}
