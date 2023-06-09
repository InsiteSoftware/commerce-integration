﻿// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class SalesOrder
    {
        public string Id { get; set; }

        public int RowNumber { get; set; }

        public string Note { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool Approved { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string BaseCurrencyID { get; set; }

        public Billtoaddress BillToAddress { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool BillToAddressOverride { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool BillToAddressValidated { get; set; }

        public Billtocontact BillToContact { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool BillToContactOverride { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string CashAccount { get; set; }

        public Commissions Commissions { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal ControlTotal { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool CreditHold { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string CurrencyID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal CurrencyRate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string CurrencyRateTypeID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string CustomerID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string CustomerOrder { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? Date { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Description { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string DestinationWarehouseID { get; set; }

        public List<Detail> Details { get; set; } = new List<Detail>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool DisableAutomaticDiscountUpdate { get; set; }

        public List<Discountdetail> DiscountDetails { get; set; } = new List<Discountdetail>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? EffectiveDate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ExternalRef { get; set; }

        public Financialsettings FinancialSettings { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool Hold { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool IsTaxValid { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? LastModified { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string LocationID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool NewCard { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal OrderedQty { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string OrderNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal OrderTotal { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string OrderType { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string PaymentCardIdentifier { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string PaymentMethod { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string PaymentProfileID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string PaymentRef { get; set; }

        public List<Payment> Payments { get; set; } = new List<Payment>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string PreAuthorizationNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal PreAuthorizedAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string PreferredWarehouseID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Project { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal ReciprocalRate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? RequestedOn { get; set; }

        public List<Shipment> Shipments { get; set; } = new List<Shipment>();

        public Shippingsettings ShippingSettings { get; set; }

        public Shiptoaddress ShipToAddress { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool ShipToAddressOverride { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool ShipToAddressValidated { get; set; }

        public Shiptocontact ShipToContact { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool ShipToContactOverride { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ShipVia { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Status { get; set; }

        public List<Taxdetail> TaxDetails { get; set; } = new List<Taxdetail>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal TaxTotal { get; set; }

        public Totals Totals { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal VATExemptTotal { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal VATTaxableTotal { get; set; }
    }

    public class Billtoaddress
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string AddressLine1 { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string AddressLine2 { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string City { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Country { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string PostalCode { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string State { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool Validated { get; set; }
    }

    public class Billtocontact
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Attention { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string BusinessName { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Email { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Phone1 { get; set; }
    }

    public class Commissions
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string DefaultSalesperson { get; set; }

        public List<Salesperson> SalesPersons { get; set; } = new List<Salesperson>();
    }

    public class File
    {
        public string id { get; set; }

        public string filename { get; set; }

        public string href { get; set; }
    }

    public class Salesperson
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal CommissionableAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal CommissionAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal CommissionPercent { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string SalespersonID { get; set; }
    }

    public class Financialsettings
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool BillSeparately { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Branch { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? CashDiscountDate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string CustomerTaxZone { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? DueDate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string EntityUsageType { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? InvoiceDate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string InvoiceNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string OriginalOrderNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string OriginalOrderType { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool OverrideTaxZone { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Owner { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string PostPeriod { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Terms { get; set; }
    }

    public class Shippingsettings
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? CancelByDate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool Canceled { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string FOBPodecimal { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool GroundCollect { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool Insurance { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string PreferredWarehouseID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal Priority { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool ResidentialDelivery { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool SaturdayDelivery { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? ScheduledShipmentDate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ShippingRule { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ShippingTerms { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ShippingZone { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool ShipSeparately { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ShipVia { get; set; }

        public Shopforrates ShopForRates { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool UseCustomersAccount { get; set; }
    }

    public class Shopforrates
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool IsManualPackage { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal OrderWeight { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal PackageWeight { get; set; }
    }

    public class Shiptoaddress
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string AddressLine1 { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string AddressLine2 { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string City { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Country { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string PostalCode { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string State { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool Validated { get; set; }
    }

    public class Shiptocontact
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Attention { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string BusinessName { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Email { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Phone1 { get; set; }
    }

    public class Totals
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal DiscountTotal { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal Freight { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal FreightCost { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool FreightCostIsuptodate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string FreightTaxCategory { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal LineTotalAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal MiscTotalAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal OrderVolume { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal OrderWeight { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal PackageWeight { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal PremiumFreight { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal TaxTotal { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal UnbilledAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal UnbilledQty { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal UnpaidBalance { get; set; }
    }

    public class Detail
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Account { get; set; }

        public List<Allocation> Allocations { get; set; } = new List<Allocation>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string AlternateID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool AutoCreateIssue { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal AverageCost { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Branch { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool CalculateDiscountsOnImport { get; set; }

        [JsonIgnore]
        public bool Commissionable
        {
            get { return this.CommissionableValue.GetValueOrDefault(); }
            set { this.CommissionableValue = value; }
        }

        [JsonProperty("Commissionable")]
        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        private bool? CommissionableValue { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool Completed { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string CostCode { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal DiscountAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string DiscountCode { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal DiscountedUnitPrice { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal DiscountPercent { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal ExtendedPrice { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool FreeItem { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string InventoryID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string LastModifiedDate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string LineDescription { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public int LineNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string LineType { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Location { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool ManualDiscount { get; set; }

        [JsonIgnore]
        public bool MarkForPO
        {
            get { return this.MarkForPOValue.GetValueOrDefault(); }
            set { this.MarkForPOValue = value; }
        }

        [JsonProperty("MarkForPO")]
        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        private bool? MarkForPOValue { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal OpenQty { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Operation { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal OrderQty { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal OvershipThreshold { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string POSource { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ProjectTask { get; set; }

        public Purchasingsettings PurchasingSettings { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal QtyOnShipments { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ReasonCode { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? RequestedOn { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string SalespersonID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? ShipOn { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ShippingRule { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Subitem { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string TaxCategory { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal UnbilledAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal UndershipThreshold { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal UnitCost { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal UnitPrice { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string UOM { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string WarehouseID { get; set; }
    }

    public class Purchasingsettings
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string POSiteID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string POSource { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string VendorID { get; set; }
    }

    public class Allocation
    {
        public string id { get; set; }
        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool Allocated { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string AllocWarehouseID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool Completed { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? ExpirationDate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string InventoryID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public int LineNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string LocationID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string LotSerialNbr { get; set; }
        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string OrderNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string OrderType { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal Qty { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal QtyOnShipments { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal QtyReceived { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string RelatedDocument { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? ShipOn { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public int SplitLineNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Subitem { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string UOM { get; set; }
    }

    public class Discountdetail
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Description { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal DiscountableAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal DiscountableQty { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal DiscountAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string DiscountCode { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal DiscountPercent { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ExternalDiscountCode { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string FreeItem { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal FreeItemQty { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool ManualDiscount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string SequenceID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool SkipDiscount { get; set; }

        public Type Type { get; set; }
    }

    public class Payment
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal AppliedToOrder { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal Balance { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string CashAccount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string CurrencyID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string DocType { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string OrderNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string OrderType { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal PaymentAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string PaymentMethod { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string PaymentRef { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ReferenceNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Status { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal TransferredtoInvoice { get; set; }
    }

    public class Shipment
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string InventoryDocType { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string InventoryRefNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string InvoiceNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string InvoiceType { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public DateTime? ShipmentDate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ShipmentNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string ShipmentType { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal ShippedQty { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal ShippedVolume { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal ShippedWeight { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string Status { get; set; }
    }

    public class Taxdetail
    {
        public string id { get; set; }

        public int rowNumber { get; set; }

        public string note { get; set; }

        public List<File> files { get; set; } = new List<File>();

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool IncludeInVATExemptTotal { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public int LineNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string OrderNbr { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string OrderType { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool PendingVAT { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public int RecordID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool ReverseVAT { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public bool StatisticalVAT { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal TaxableAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal TaxAmount { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string TaxID { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public decimal TaxRate { get; set; }

        [JsonConverter(typeof(ValuePropertyJsonConverter))]
        public string TaxType { get; set; }
    }
}