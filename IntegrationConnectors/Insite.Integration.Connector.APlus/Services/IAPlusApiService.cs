namespace Insite.Integration.Connector.APlus.Services;

using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.AccountsReceivableSummary;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;

internal interface IAPlusApiService
{
    /// <summary>
    /// Executes the ARSummary web service call with the provided xml request.
    /// </summary>
    /// <param name="accountsReceivableSummaryRequest">The <see cref="AccountsReceivableSummaryRequest"/>.</param>
    /// <returns>The <see cref="AccountsReceivableSummaryResponse"/> received from the web service.</returns>
    AccountsReceivableSummaryResponse AccountsReceivableSummary(
        AccountsReceivableSummaryRequest accountsReceivableSummaryRequest
    );

    /// <summary>
    /// Executes the GetAvail web service call with the provided <see cref="LineItemPriceAndAvailabilityRequest"/>.
    /// </summary>
    /// <param name="lineItemPriceAndAvailabilityRequest">The <see cref="LineItemPriceAndAvailabilityRequest"/>.</param>
    /// <returns>The <see cref="LineItemPriceAndAvailabilityResponse"/> received from the web service.</returns>
    LineItemPriceAndAvailabilityResponse LineItemPriceAndAvailability(
        LineItemPriceAndAvailabilityRequest lineItemPriceAndAvailabilityRequest
    );

    /// <summary>
    /// Executes the CreateOrder web service call with the provided <see cref="CreateOrderRequest"/>.
    /// </summary>
    /// <param name="createOrderRequest">The <see cref="CreateOrderRequest"/>.</param>
    /// <returns>The <see cref="CreateOrderResponse"/> received from the web service.</returns>
    CreateOrderResponse CreateOrder(CreateOrderRequest createOrderRequest);
}
