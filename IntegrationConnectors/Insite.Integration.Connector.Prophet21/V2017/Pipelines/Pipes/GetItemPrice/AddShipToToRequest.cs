namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddShipToToRequest : IPipe<GetItemPriceParameter, GetItemPriceResult>
{
    public int Order => 400;

    public GetItemPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetItemPriceParameter parameter,
        GetItemPriceResult result
    )
    {
        var shipToId = parameter.PricingServiceParameters.FirstOrDefault()?.ShipToId;
        if (shipToId == null)
        {
            return result;
        }

        var shipTo = unitOfWork.GetRepository<Customer>().Get(shipToId.Value);
        if (shipTo == null)
        {
            throw new ArgumentException($"Customer not found. Id: {shipToId.Value}.");
        }

        var erpSequence = shipTo.PricingCustomer?.ErpSequence ?? shipTo.ErpSequence;
        if (string.IsNullOrEmpty(erpSequence))
        {
            // no default pricing customer and not yet submitted to erp
            return result;
        }

        result.GetItemPriceRequest.Request.ShipToID = erpSequence;

        return result;
    }
}
