namespace Insite.Integration.Connector.Prophet21.Services;

using Insite.Core.Interfaces.Dependency;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetMyAccountOpenAR;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;

internal interface IProphet21ApiService : IDependency
{
    GetItemPrice GetItemPrice(
        IntegrationConnection integrationConnection,
        GetItemPrice getItemPrice
    );

    GetMyAccountOpenAR GetMyAccountOpenAR(
        IntegrationConnection integrationConnection,
        GetMyAccountOpenAR getMyAccountOpenAR
    );

    OrderImport OrderImport(IntegrationConnection integrationConnection, OrderImport orderImport);

    GetCartSummary GetCartSummary(
        IntegrationConnection integrationConnection,
        GetCartSummary getCartSummary
    );

    ArrayOfContact GetContacts(IntegrationConnection integrationConnection, string emailAddress);

    Contact CreateContact(IntegrationConnection integrationConnection, Contact contact);
}
