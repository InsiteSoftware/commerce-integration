namespace Insite.Integration.Connector.Acumatica.Services;

using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.CustomerPaymentMethod;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.InventoryAllocationInquiry;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;

internal interface IAcumaticaApiService
{
    void Login();

    void Logout();

    InventoryAllocationInquiry InventoryAllocationInquiry(
        InventoryAllocationInquiry inventoryAllocationInquiry
    );

    SalesOrder SalesOrder(SalesOrder salesOrder);

    CustomerPaymentMethod CustomerPaymentMethod(CustomerPaymentMethod customerPaymentMethod);
}
