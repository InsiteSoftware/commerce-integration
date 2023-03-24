namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.OEPricingMultipleV4;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.OEPricingMultipleV4;

public sealed class CreateInitialRequest
    : IPipe<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    public int Order => 100;

    public OEPricingMultipleV4Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV4Parameter parameter,
        OEPricingMultipleV4Result result
    )
    {
        result.OEPricingMultipleV4Request = new OEPricingMultipleV4Request
        {
            SendFullQtyOnOrder = true
        };

        return result;
    }
}
