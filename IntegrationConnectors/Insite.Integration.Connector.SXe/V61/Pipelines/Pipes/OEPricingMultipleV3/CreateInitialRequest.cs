namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.OEPricingMultipleV3;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;

public sealed class CreateInitialRequest
    : IPipe<OEPricingMultipleV3Parameter, OEPricingMultipleV3Result>
{
    public int Order => 100;

    public OEPricingMultipleV3Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV3Parameter parameter,
        OEPricingMultipleV3Result result
    )
    {
        result.OEPricingMultipleV3Request = new OEPricingMultipleV3Request
        {
            sendFullQtyOnOrder = true
        };

        return result;
    }
}
