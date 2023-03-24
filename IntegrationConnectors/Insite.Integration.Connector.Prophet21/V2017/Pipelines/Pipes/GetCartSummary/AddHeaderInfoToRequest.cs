namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddHeaderInfoToRequest : IPipe<GetCartSummaryParameter, GetCartSummaryResult>
{
    public int Order => 200;

    public GetCartSummaryResult Execute(
        IUnitOfWork unitOfWork,
        GetCartSummaryParameter parameter,
        GetCartSummaryResult result
    )
    {
        result.GetCartSummaryRequest.Request.WebReferenceNumber = parameter
            .CustomerOrder
            .OrderNumber;
        result.GetCartSummaryRequest.Request.PONumber = parameter.CustomerOrder.CustomerPO;

        if (
            parameter.CustomerOrder.FulfillmentMethod.EqualsIgnoreCase(
                FulfillmentMethod.PickUp.ToString()
            )
        )
        {
            result.GetCartSummaryRequest.Request.CartRequiredDate =
                parameter.CustomerOrder.RequestedPickupDate?.ToString("yyyy-MM-dd");
        }
        else
        {
            result.GetCartSummaryRequest.Request.CartRequiredDate =
                parameter.CustomerOrder.RequestedDeliveryDate?.ToString("yyyy-MM-dd");
        }

        return result;
    }
}
