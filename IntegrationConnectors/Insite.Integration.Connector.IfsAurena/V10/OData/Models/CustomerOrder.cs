namespace Insite.Integration.Connector.IfsAurena.V10.OData.Models;

using System;
using Newtonsoft.Json;

public class CustomerOrder
{
    public string OrderNo { get; set; }

    public string AuthorizeCode { get; set; }

    public string BillAddrNo { get; set; }

    public string Contract { get; set; }

    public string Company { get; set; }

    public string CurrencyCode { get; set; }

    public string CustomerNo { get; set; }

    public string CustomerNoPay { get; set; }

    public string CustomerNoPayAddrNo { get; set; }

    public string DeliveryTerms { get; set; }

    public string DistrictCode { get; set; }

    public string LanguageCode { get; set; }

    public string MarketCode { get; set; }

    public decimal? NoteId { get; set; }

    public string OrderId { get; set; }

    public string PayTermId { get; set; }

    public decimal? PreAccountingId { get; set; }

    public string PrintControlCode { get; set; }

    public string RegionCode { get; set; }

    public string SalesmanCode { get; set; }

    public string ShipAddrNo { get; set; }

    public string ShipViaCode { get; set; }

    public string CustomerPoNo { get; set; }

    public string CustRef { get; set; }

    public DateTime? DateEntered { get; set; }

    public decimal? DeliveryLeadtime { get; set; }

    public string LabelNote { get; set; }

    public string NoteText { get; set; }

    public string TaxLiability { get; set; }

    public string DeliveryCountryCode { get; set; }

    [JsonConverter(typeof(UtcDateTimeOffsetConverter))]
    public DateTimeOffset? WantedDeliveryDate { get; set; }

    public string InternalPoNo { get; set; }

    public string RouteId { get; set; }

    public string AgreementId { get; set; }

    public string ForwardAgentId { get; set; }

    public string ExternalRef { get; set; }

    public string ProjectId { get; set; }

    public decimal? Priority { get; set; }

    public DateTime? PayTermBaseDate { get; set; }

    public decimal? CaseId { get; set; }

    public decimal? TaskId { get; set; }

    public string SalesContractNo { get; set; }

    public decimal? ContractRevSeq { get; set; }

    public decimal? ContractLineNo { get; set; }

    public decimal? ContractItemNo { get; set; }

    public decimal? ProposedPrepaymentAmount { get; set; }

    public string TaxIdNo { get; set; }

    public DateTime? TaxIdValidatedDate { get; set; }

    public string ClassificationStandard { get; set; }

    public string CurrencyRateType { get; set; }

    public string DelTermsLocation { get; set; }

    public string InternalRef { get; set; }

    public string InternalPoLabelNote { get; set; }

    public string RebateCustomer { get; set; }

    public string FreightMapId { get; set; }

    public string ZoneId { get; set; }

    public string FreightPriceListNo { get; set; }

    public string CustCalendarId { get; set; }

    public string ExtTransportCalendarId { get; set; }

    public string CustomsValueCurrency { get; set; }

    public decimal? PickingLeadtime { get; set; }

    public string VendorNo { get; set; }

    public string ReplicateChanges { get; set; }

    public string ChangeRequest { get; set; }

    public string QuotationNo { get; set; }

    public bool? IsNewCustomer { get; set; }

    public string CompanyCurrencyCode { get; set; }

    public decimal? AdditionalDiscount { get; set; }

    public bool? DocumentText { get; set; }

    public string ShipmentType { get; set; }

    public decimal? FixDelivFreight { get; set; }

    public bool? ApplyFixDelivFreightDb { get; set; }

    public bool? IntrastatExemptDb { get; set; }

    public bool? ConfirmDeliveriesDb { get; set; }

    public bool? CheckSalesGrpDelivConfDb { get; set; }

    public bool? DelayCogsToDelivConfDb { get; set; }

    public bool? UsePreShipDelNoteDb { get; set; }

    public bool? PickInventoryTypeDb { get; set; }

    public string CancelReason { get; set; }

    public string BlockedReason { get; set; }

    public string CustomerNoPayRef { get; set; }

    public bool? JinsuiInvoiceDb { get; set; }

    public bool? UsePriceInclTax { get; set; }

    public string BlockedTypeDb { get; set; }

    public string BusinessOpportunityNo { get; set; }

    public bool? SmConnectionDb { get; set; }

    public bool? SchedulingConnectionDb { get; set; }

    public bool? AdvancePrepaymInvExists { get; set; }

    public bool? LimitSalesToAssortmentsDb { get; set; }

    public bool? OrderConfFlagDb { get; set; }

    public bool? PackListFlagDb { get; set; }

    public bool? PickListFlagDb { get; set; }

    public bool? OrderConfDb { get; set; }

    public bool? DelNotePrinted { get; set; }

    public bool? PickListPrinted { get; set; }

    public bool? SummarizedSourceLinesDb { get; set; }

    public bool? SummarizedFreightChargesDb { get; set; }

    public string SupplyCountryDb { get; set; }

    public bool? B2bOrderDb { get; set; }

    public string MainRepresentativeId { get; set; }

    public bool? ChargesExist { get; set; }

    public string AllowedOperations { get; set; }

    public string TaxCalcMethod { get; set; }

    public string BusinessObjectId { get; set; }

    public string ConnectionId { get; set; }

    public string SourceRef { get; set; }

    public string DelAddrName { get; set; }

    public string BillAddrName { get; set; }

    public bool? AddrFlagDb { get; set; }

    public bool? DummySingleOccur { get; set; }

    public string TaxFreeTaxCode { get; set; }

    public string OverruleLimitSaleToAssort { get; set; }

    public string CopyAddrToLine { get; set; }

    public string ChangeLineDate { get; set; }

    [JsonConverter(typeof(UtcDateTimeOffsetConverter))]
    public DateTimeOffset? PlannedDeliveryDate { get; set; }

    public string UpdatePriceEffectiveDate { get; set; }

    public string DisconnectExpLicense { get; set; }

    public decimal? TotalAmount { get; set; }

    public string RebateCustomerName { get; set; }

    public string CustomerName { get; set; }

    public string CustomerNoPayName { get; set; }

    public string CustRefName { get; set; }

    public string CustomerNoPayReferenceName { get; set; }

    public string SalesPartSalesmanName { get; set; }

    public string PaymentTermDescription { get; set; }

    public decimal? ShipmentConnectedLinesExist { get; set; }

    public string VendorName { get; set; }

    public string AgreementDescription { get; set; }

    public string RepresentativeName { get; set; }

    public string CustomerBranch { get; set; }

    public string WantedDeliveryDateChanged { get; set; }

    public string FreightZoneDescription { get; set; }

    public string ForwardName { get; set; }

    public string TaxFreeTaxCodeDescription { get; set; }

    public string ReplicateValuesModified { get; set; }

    public string ChangedAttribNotInPol { get; set; }

    public string PeggingExist { get; set; }
}
