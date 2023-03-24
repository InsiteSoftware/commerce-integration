namespace Insite.Integration.Connector.IfsAurena.Services;

using Insite.Core.Interfaces.Dependency;
using Insite.Core.Services;
using Insite.Data.Entities;

internal interface IIfsAurenaClient : IDependency
{
    (ResultCode resultCode, string response) GetInventoryStock(
        IntegrationConnection integrationConnection,
        string request
    );

    (ResultCode resultCode, string response) GetPricing(
        IntegrationConnection integrationConnection,
        string request
    );

    (ResultCode resultCode, string response) CleanPriceQuery(
        IntegrationConnection integrationConnection
    );

    (ResultCode resultCode, string response) SubmitOrder(
        IntegrationConnection integrationConnection,
        string request
    );

    (ResultCode resultCode, string response) SubmitOrderLine(
        IntegrationConnection integrationConnection,
        string erpOrderNumber,
        string request
    );

    (ResultCode resultCode, string response) SubmitOrderCharge(
        IntegrationConnection integrationConnection,
        string erpOrderNumber,
        string request
    );

    (ResultCode resultCode, string response) SubmitCreditCardDetails(
        IntegrationConnection integrationConnection,
        string erpOrderNumber,
        string request
    );

    (ResultCode resultCode, string response) SubmitAuthorizationCode(
        IntegrationConnection integrationConnection,
        string request
    );
}
