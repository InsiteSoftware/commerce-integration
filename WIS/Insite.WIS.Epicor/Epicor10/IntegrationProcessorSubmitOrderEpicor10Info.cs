namespace Insite.WIS.Epicor.Epicor10;

public class IntegrationProcessorSubmitOrderEpicor10Info
{
    public string CompanyNumber { get; set; }

    public bool SubmitAllPaymentInfo { get; set; }

    public bool SubmitToReviewStatus { get; set; }

    public bool PromoMiscCharge { get; set; }

    public bool FreightEstimated { get; set; }

    public bool SubmitSaleTransaction { get; set; }

    public string FreightCode { get; set; }

    public string PromoMiscChargeCode { get; set; }

    public bool UseStaticCustomer { get; set; }

    public string StaticCustomerNumber { get; set; }

    public string CreditCardCashAccountId { get; set; }

    public string CreditCardChart { get; set; }
}
