namespace Insite.Integration.Connector.Ifs.Services;

using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;

internal interface IIfsApiService
{
    /// <summary>
    /// Executes the getCustomerPrice with the provided <see cref="customerPriceRequest"/>.
    /// </summary>
    /// <param name="customerPriceRequest">The <see cref="customerPriceRequest"/></param>
    /// <returns>The <see cref="customerPriceResponse"/></returns>
    customerPriceResponse GetCustomerPrice(customerPriceRequest customerPriceRequest);

    /// <summary>
    /// Executes the getPartAvailability with the provided <see cref="partAvailabilityRequest"/>.
    /// </summary>
    /// <param name="partAvailabilityRequest">The <see cref="partAvailabilityRequest"/></param>
    /// <returns>The <see cref="partAvailabilityResponse"/></returns>
    partAvailabilityResponse GetPartAvailability(partAvailabilityRequest partAvailabilityRequest);

    /// <summary>
    /// Executes the createCustomerOrder with the provided <see cref="customerOrder"/>.
    /// </summary>
    /// <param name="customerOrder">The <see cref="customerOrder"/></param>
    /// <returns>The <see cref="orderResponse"/></returns>
    orderResponse CreateCustomerOrder(customerOrder customerOrder);
}
