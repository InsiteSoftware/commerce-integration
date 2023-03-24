namespace Insite.Integration.Connector.IfsAurena.V10.OData.Models;

using Newtonsoft.Json;

public class CustomerOrderCharge
{
    public string OrderNo { get; set; }

    public decimal? SequenceNo { get; set; }

    public string LineNo { get; set; }

    public string RelNo { get; set; }

    public decimal? LineItemNo { get; set; }

    public string Contract { get; set; }

    public string ChargeType { get; set; }

    public decimal? ChargeAmount { get; set; }

    public decimal? ChargeAmountInclTax { get; set; }

    public decimal? BaseChargeAmount { get; set; }

    public decimal? BaseChargeAmtInclTax { get; set; }

    public decimal? ChargeCost { get; set; }

    public decimal? ChargedQty { get; set; }

    public string SalesUnitMeas { get; set; }

    public decimal? InvoicedQty { get; set; }

    public string TaxCode { get; set; }

    public string TaxClassId { get; set; }

    public decimal? NoteId { get; set; }

    public string Company { get; set; }

    public decimal? ShipmentId { get; set; }

    public string ChargePriceListNo { get; set; }

    public decimal? Charge { get; set; }

    public decimal? ChargeCostPercent { get; set; }

    public decimal? CampaignId { get; set; }

    public decimal? DealId { get; set; }

    public string DeliveryType { get; set; }

    public decimal? CurrencyRate { get; set; }

    public decimal? StatisticalChargeDiff { get; set; }

    public string TaxCalcStructureId { get; set; }

    public string ChargeTypeDescription { get; set; }

    public string ChargeGroupDesc { get; set; }

    public decimal? ChargeBasisCurr { get; set; }

    public bool? CollectDb { get; set; }

    public string TaxLiability { get; set; }

    public string TaxLiabilityType { get; set; }

    public string TaxCodeDescription { get; set; }

    public bool? MultipleTaxLines { get; set; }

    public decimal? NetAmtBase { get; set; }

    public decimal? NetAmtCurr { get; set; }

    public decimal? GrossAmtBase { get; set; }

    public decimal? GrossAmtCurr { get; set; }

    public decimal? TotalChargeCost { get; set; }

    public bool? PrintChargeTypeDb { get; set; }

    public bool? PrintCollectChargeDb { get; set; }

    public bool? DocumentText { get; set; }

    [JsonProperty(PropertyName = "IntrastatExempt")]
    public bool? IntrastatExemptDb { get; set; }

    [JsonProperty(PropertyName = "UnitCharge")]
    public bool? UnitChargeDb { get; set; }

    public decimal? FreightFactor { get; set; }

    public decimal? TaxAmountCurrency { get; set; }

    public string DeliveryAddress { get; set; }

    public bool? TaxCodeFlag { get; set; }

    public string FetchTaxCodes { get; set; }

    public bool? TaxEdited { get; set; }

    public string OldTaxCalcStructureId { get; set; }
}
