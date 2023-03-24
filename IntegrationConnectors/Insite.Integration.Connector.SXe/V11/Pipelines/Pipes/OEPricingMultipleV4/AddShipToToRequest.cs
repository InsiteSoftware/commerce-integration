namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.OEPricingMultipleV4;

using System;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;

public sealed class AddShipToToRequest
    : IPipe<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    public int Order => 300;

    public OEPricingMultipleV4Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV4Parameter parameter,
        OEPricingMultipleV4Result result
    )
    {
        result.OEPricingMultipleV4Request.Request.ShipTo = GetShipToNumber(unitOfWork, parameter);

        return result;
    }

    private static string GetShipToNumber(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV4Parameter parameter
    )
    {
        var shipToId = parameter.PricingServiceParameters.FirstOrDefault()?.ShipToId;
        if (shipToId == null)
        {
            return string.Empty;
        }

        var shipTo = unitOfWork.GetRepository<Customer>().Get(shipToId.Value);
        if (shipTo == null)
        {
            throw new ArgumentException($"No customer found with id {shipToId.Value}.");
        }

        var erpSequence = shipTo.PricingCustomer?.ErpSequence ?? shipTo.ErpSequence;
        if (string.IsNullOrEmpty(erpSequence))
        {
            // no default pricing customer and not yet submitted to erp
            return string.Empty;
        }

        return erpSequence;
    }
}
