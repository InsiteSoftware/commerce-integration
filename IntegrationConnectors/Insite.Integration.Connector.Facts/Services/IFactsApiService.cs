namespace Insite.Integration.Connector.Facts.Services;

using Insite.Integration.Connector.Facts.V9.Api.Models.CustomerSummary;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderTotal;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;

internal interface IFactsApiService
{
    PriceAvailabilityResponse PriceAvailability(PriceAvailabilityRequest priceAvailabilityRequest);

    CustomerSummaryResponse CustomerSummary(CustomerSummaryRequest customerSummaryRequest);

    OrderTotalResponse OrderTotal(OrderTotalRequest orderTotalRequest);

    OrderLoadResponse OrderLoad(OrderLoadRequest orderLoadRequest);
}
