namespace Insite.Integration.Connector.IfsAurena.V10.OData.Models;

using System;
using Newtonsoft.Json;

public class CustomerOrderLine
{
    public string OrderNo { get; set; }

    public string LineNo { get; set; }

    public string RelNo { get; set; }

    public decimal? LineItemNo { get; set; }

    public string Contract { get; set; }

    public string Company { get; set; }

    public string CatalogNo { get; set; }

    public decimal? NoteId { get; set; }

    public string PartNo { get; set; }

    public decimal? PreAccountingId { get; set; }

    public string SalesUnitMeas { get; set; }

    public decimal? BaseSaleUnitPrice { get; set; }

    public decimal? BaseUnitPriceInclTax { get; set; }

    public decimal? BuyQtyDue { get; set; }

    public string CatalogDesc { get; set; }

    public string CatalogType { get; set; }

    public decimal? ConvFactor { get; set; }

    public decimal? Cost { get; set; }

    public decimal? CurrencyRate { get; set; }

    [JsonConverter(typeof(UtcDateTimeOffsetConverter))]
    public DateTimeOffset? DateEntered { get; set; }

    public decimal? Discount { get; set; }

    public decimal? LineTotalQty { get; set; }

    public string NoteText { get; set; }

    public decimal? OrderDiscount { get; set; }

    [JsonConverter(typeof(UtcDateTimeOffsetConverter))]
    public DateTimeOffset? PlannedDeliveryDate { get; set; }

    public DateTime? PlannedDueDate { get; set; }

    public DateTime? SupplySiteDueDate { get; set; }

    public decimal? PriceConvFactor { get; set; }

    [JsonConverter(typeof(UtcDateTimeOffsetConverter))]
    public DateTimeOffset? PromisedDeliveryDate { get; set; }

    public decimal? QtyAssigned { get; set; }

    public decimal? QtyInvoiced { get; set; }

    public decimal? QtyOnOrder { get; set; }

    public decimal? QtyReturned { get; set; }

    public decimal? QtyToShip { get; set; }

    public decimal? QtyShort { get; set; }

    public DateTime? RealShipDate { get; set; }

    public decimal? RevisedQtyDue { get; set; }

    public decimal? SaleUnitPrice { get; set; }

    public decimal? UnitPriceInclTax { get; set; }

    public string SupplyCode { get; set; }

    [JsonConverter(typeof(UtcDateTimeOffsetConverter))]
    public DateTimeOffset? WantedDeliveryDate { get; set; }

    public string DeliveryType { get; set; }

    public string VendorNo { get; set; }

    public string TaxCode { get; set; }

    public string TaxClassId { get; set; }

    public string CustomerPartNo { get; set; }

    public decimal? CustomerPartConvFactor { get; set; }

    public string CustomerPartUnitMeas { get; set; }

    public decimal? CustomerPartBuyQty { get; set; }

    [JsonConverter(typeof(UtcDateTimeOffsetConverter))]
    public DateTimeOffset? PlannedShipDate { get; set; }

    public string PlannedShipPeriod { get; set; }

    public string CustomerNo { get; set; }

    public string ConsignmentStock { get; set; }

    public decimal? CloseTolerance { get; set; }

    public string PriceListNo { get; set; }

    public string ChargedItem { get; set; }

    public string DemandOrderRef1 { get; set; }

    public string DemandOrderRef2 { get; set; }

    public string DemandOrderRef3 { get; set; }

    public string CreateSmObjectOption { get; set; }

    public string DefaultAddrFlag { get; set; }

    public string AddrFlag { get; set; }

    public string ShipAddrNo { get; set; }

    public string RouteId { get; set; }

    public string ForwardAgentId { get; set; }

    public string ShipViaCode { get; set; }

    public string DeliveryTerms { get; set; }

    public decimal? DeliveryLeadtime { get; set; }

    public string DistrictCode { get; set; }

    public string RegionCode { get; set; }

    public decimal? DesiredQty { get; set; }

    public string PurchasePartNo { get; set; }

    public string StagedBilling { get; set; }

    public string TaxLiability { get; set; }

    public string OriginalPartNo { get; set; }

    public string SupSmContract { get; set; }

    public string SupSmObject { get; set; }

    public string SmConnection { get; set; }

    public decimal? PartPrice { get; set; }

    public decimal? CalcCharPrice { get; set; }

    public decimal? CharPrice { get; set; }

    public string PriceSource { get; set; }

    public string PriceFreeze { get; set; }

    public string DockCode { get; set; }

    public string SubDockCode { get; set; }

    public string RefId { get; set; }

    public string LocationNo { get; set; }

    public string ConfigurationId { get; set; }

    public DateTime? PriceEffectivityDate { get; set; }

    public decimal? ConfiguredLinePriceId { get; set; }

    public DateTime? LatestReleaseDate { get; set; }

    public string JobId { get; set; }

    public decimal? CustWarrantyId { get; set; }

    public string PriceSourceId { get; set; }

    public string PriceUnitMeas { get; set; }

    public string ConditionCode { get; set; }

    public decimal? AdditionalDiscount { get; set; }

    public string OwningCustomerNo { get; set; }

    public string OriginatingRelNo { get; set; }

    public decimal? OriginatingLineItemNo { get; set; }

    public string ReleasePlanning { get; set; }

    public string ReplicateChanges { get; set; }

    public string ChangeRequest { get; set; }

    public string SupplySite { get; set; }

    public decimal? ActivitySeq { get; set; }

    public string ProjectId { get; set; }

    public string DeliverToCustomerNo { get; set; }

    public string Contact { get; set; }

    public decimal? InputQty { get; set; }

    public string InputUnitMeas { get; set; }

    public decimal? InputConvFactor { get; set; }

    public string InputVariableValues { get; set; }

    public decimal? DeliverySequence { get; set; }

    public string CancelReason { get; set; }

    public string CustomerPoLineNo { get; set; }

    public string CustomerPoRelNo { get; set; }

    public DateTime? FirstActualShipDate { get; set; }

    public string TaxIdNo { get; set; }

    public DateTime? TaxIdValidatedDate { get; set; }

    [JsonConverter(typeof(UtcDateTimeOffsetConverter))]
    public DateTimeOffset? TargetDate { get; set; }

    public string DelTermsLocation { get; set; }

    public string ClassificationPartNo { get; set; }

    public string ClassificationUnitMeas { get; set; }

    public string ClassificationStandard { get; set; }

    public string FreightMapId { get; set; }

    public string ZoneId { get; set; }

    public string FreightPriceListNo { get; set; }

    public decimal? AdjustedWeightNet { get; set; }

    public decimal? AdjustedWeightGross { get; set; }

    public string PartLevelId { get; set; }

    public string CustomerLevelId { get; set; }

    public decimal? LoadId { get; set; }

    public decimal? CustomsValue { get; set; }

    public string CustCalendarId { get; set; }

    public string ExtTransportCalendarId { get; set; }

    public bool? RelMtrlPlanning { get; set; }

    public decimal? InvertedConvFactor { get; set; }

    public string EndCustomerId { get; set; }

    public decimal? PickingLeadtime { get; set; }

    public string ShipmentType { get; set; }

    public string PackingInstructionId { get; set; }

    public string OriginatingCoLangCode { get; set; }

    public string ChangedAttribNotInPol { get; set; }

    public string EvaluateDefaultInfo { get; set; }

    public decimal? CustPartInvertConvFact { get; set; }

    public decimal? FreeOfChargeTaxBasis { get; set; }

    public string TaxCalcStructureId { get; set; }

    public string LineNumber { get; set; }

    public string RelNumber { get; set; }

    public string LineItemNumber { get; set; }

    public string CountryCode { get; set; }

    public decimal? OpenShipmentQty { get; set; }

    public decimal? QtyPicked { get; set; }

    public decimal? QtyShipped { get; set; }

    public decimal? QtyShipdiff { get; set; }

    public decimal? QtyConfirmeddiff { get; set; }

    public string DeliveryConfirmed { get; set; }

    public string InputUnitMeasGroupId { get; set; }

    public bool? AbnormalDemandDb { get; set; }

    public bool? InputUomGroup { get; set; }

    public bool? DocumentText { get; set; }

    public bool? ProvisionalPriceDb { get; set; }

    public decimal? DiscountAmountCurr { get; set; }

    public decimal? TotalOrderLineDiscount { get; set; }

    public bool? FreeOfChargeDb { get; set; }

    public decimal? CompanyBearingTaxAmountBase { get; set; }

    public bool? RebateBuilderDb { get; set; }

    public decimal? NetAmountBase { get; set; }

    public decimal? TaxAmountBase { get; set; }

    public decimal? GrossAmountBase { get; set; }

    public decimal? ContribMarginBase { get; set; }

    public string TaxCodeDescription { get; set; }

    public string TaxClassDescription { get; set; }

    public decimal? NetAmountCurr { get; set; }

    public decimal? TaxAmountCurr { get; set; }

    public decimal? GrossAmountCurr { get; set; }

    public bool? MultipleTaxLines { get; set; }

    public string PreviousTaxIdNo { get; set; }

    public bool? BlockedForInvoicingDb { get; set; }

    public bool? CustomerWarranty { get; set; }

    public string EndCustomerName { get; set; }

    public bool? Configurable { get; set; }

    public string ConfigurationStatus { get; set; }

    public string Owner { get; set; }

    public string OwnerName { get; set; }

    public decimal? DeliveredQty { get; set; }

    public decimal? AvailableQty { get; set; }

    public decimal? PickedQty { get; set; }

    public decimal? PickedQtySalesUom { get; set; }

    public decimal? DeliveredQtySalesUom { get; set; }

    public decimal? SupplySiteReservedQty { get; set; }

    public bool? MilestoneExists { get; set; }

    public string InterimOrder { get; set; }

    public string ProjectName { get; set; }

    public string ProgramId { get; set; }

    public string ProgramDescription { get; set; }

    public string SubProjectId { get; set; }

    public string SubProjectDescription { get; set; }

    public string ActivityId { get; set; }

    public string ActivityDescription { get; set; }

    public string CancellationReasonDescription { get; set; }

    public string Gtin { get; set; }

    public decimal? FreightFactor { get; set; }

    public bool? FreightFreeDb { get; set; }

    public string DemandCodeDb { get; set; }

    public string SupplyCodeDb { get; set; }

    public string DeliveryTermsDescription { get; set; }

    public string CostLevelDb { get; set; }

    public string Allowsendchg { get; set; }

    public string DopNewQtyDemand { get; set; }

    public bool? PriceSourceNetPriceDb { get; set; }

    public string DeliveryCountryCode { get; set; }

    public bool? Rental { get; set; }

    public string RentalDb { get; set; }

    public string ShipmentConnectedDb { get; set; }

    public string ConfigManuallyEntered { get; set; }

    public string PurchaseOrderNo { get; set; }

    public decimal? Linesourced { get; set; }

    public decimal? ShipmentConnectedLinesExist { get; set; }

    public string CustomersPONo { get; set; }

    public string SalesPriceGroupId { get; set; }

    public string Source { get; set; }

    public string Identity1 { get; set; }

    public string Identity2 { get; set; }

    public string Identity3 { get; set; }

    public decimal? Identity4 { get; set; }

    public decimal? SuggestedPartExists { get; set; }

    public decimal? UnformattedDiscount { get; set; }

    public string FetchTaxCodes { get; set; }

    public string FetchTaxFromDefaults { get; set; }

    public bool? BasicDataEdited { get; set; }

    public bool? PriceEdited { get; set; }

    public string DisconnectExpLic { get; set; }

    public string RaisedQuestionExpLic { get; set; }

    public string ValidatePriceEffectiveDate { get; set; }

    public string PriceEffDateValidated { get; set; }

    public decimal? QtyPickedAndShipped { get; set; }

    public decimal? QtyToReserve { get; set; }

    public decimal? QtyUnreservable { get; set; }

    public bool? Taxable { get; set; }

    public string OrderCode { get; set; }

    public string OrderState { get; set; }

    public bool? PartExists { get; set; }

    public string SendChangeRequest { get; set; }

    public string ReplicateColumnsModified { get; set; }

    public string SupplyCodeCheck { get; set; }

    public string RefresshPriceSource { get; set; }

    public string InventoryUnitMeas { get; set; }

    public string SupplySiteInventoryUnitMeas { get; set; }

    public bool? IsInvoiceCreated { get; set; }

    public decimal? AdjustedVolume { get; set; }
}
