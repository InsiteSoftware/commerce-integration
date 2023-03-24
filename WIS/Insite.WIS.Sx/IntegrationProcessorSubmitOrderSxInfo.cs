namespace Insite.WIS.Sx;

/// <summary>
/// Class to hold general information related to the Order Submit. By default these values are populated in the PopulateOrderSubmitParameters method within
/// IntegrationProcessorOrderSubmitSx. They are passed in via integration job parameters.
/// </summary>
public class IntegrationProcessorSubmitOrderSxInfo
{
    /// <summary>
    /// Will either be;
    /// 1) NewShipTo: this action will create a new ship to record in the ERP against the customer the bill-to is related to
    /// 2) AddToOrder: this action will use the customer the bill-to is related to and impant the address directly to the order header – only available
    /// if the ERP supports it.
    /// </summary>
    public string CustomerIsDropShipAction;

    /// <summary>
    /// Will either be;
    /// 1) NewCustomer: this action will create a new customer record in the ERP
    /// 2) ShipToOnGenericCustomer: this action will add a new ship-to to the Generic customer
    /// 3) AddToOrder: this action will use the customer the bill-to is related to and impant the address directly to the order header – only available
    /// if the ERP supports it.
    /// </summary>
    public string CustomerIsGuestAction;

    /// <summary>The cono in context.</summary>
    public string CompanyNumber { get; set; }

    /// <summary>Defines whether or not we will check if existing customer/po orders have been placed prior to submitting the order.</summary>
    public bool AllowDuplicateCustomerPo { get; set; }

    /// <summary>The default warehouse to use if its not defined in the ISC customer order.</summary>
    public string CustomerDefaultWarehouse { get; set; }

    /// <summary>The field on oeeh that we populate the ISC order # with. It typically is user3 but we allow variations.</summary>
    public string OrderNumberField { get; set; }
}
