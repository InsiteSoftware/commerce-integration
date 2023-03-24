namespace Insite.Integration.Connector.IfsAurena.V10.OData.Models;

using System;

public class PriceQuery
{
    public decimal? SalesQty { get; set; }

    public DateTime? PriceEffectiveDate { get; set; }

    public string Contract { get; set; }

    public string CatalogNo { get; set; }

    public string CustomerNo { get; set; }

    public decimal? AdditionalDiscount { get; set; }

    public string CustomerPartNo { get; set; }

    public string CurrencyCode { get; set; }

    public string AgreementId { get; set; }

    public string Creator { get; set; }

    public string PriceSourceId { get; set; }

    public string PartLevelId { get; set; }

    public string CustomerLevelId { get; set; }

    public string BaseCurrencyCode { get; set; }

    public decimal? SaleUnitPrice { get; set; }

    public decimal? BaseSaleUnitPrice { get; set; }

    public decimal? AccDiscount { get; set; }

    public decimal? AccDiscountAmount { get; set; }

    public decimal? BaseAccDiscountAmount { get; set; }

    public decimal? NetPriceInclAccDisc { get; set; }

    public decimal? BaseNetPriceInclAcDsc { get; set; }

    public decimal? AddDiscountAmount { get; set; }

    public decimal? BaseAddDiscountAmount { get; set; }

    public decimal? GroupDiscount { get; set; }

    public decimal? GroupDiscountAmount { get; set; }

    public decimal? BaseGroupDiscountAmount { get; set; }

    public decimal? NetPrice { get; set; }

    public decimal? BaseNetPrice { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? BaseTotalAmount { get; set; }

    public decimal? TotalCost { get; set; }

    public decimal? BaseTotalCost { get; set; }

    public decimal? EstContrMargin { get; set; }

    public decimal? BaseEstContrMargin { get; set; }

    public decimal? TotalDiscount { get; set; }

    public decimal? EstContrMarginRate { get; set; }

    public decimal? PartCost { get; set; }

    public string CustomerLevel { get; set; }

    public string PartLevel { get; set; }

    public string PriceSource { get; set; }

    public string ConditionCode { get; set; }

    public string Source { get; set; }

    public bool? RebateAgreement { get; set; }

    public decimal? PriceQty { get; set; }

    public string SourceRef1 { get; set; }

    public string SourceRef2 { get; set; }

    public string SourceRef3 { get; set; }

    public decimal? SourceRef4 { get; set; }

    public bool? PriceSourceNetPrice { get; set; }

    public bool? UsePriceInclTax { get; set; }

    public bool? UsePriceInclTaxDb { get; set; }

    public string PriceTree { get; set; }

    public string HierarchyId { get; set; }

    public string HierarchyLevelNo { get; set; }

    public string Company { get; set; }

    public bool? PriceFreeze { get; set; }
}
