﻿// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.Facts.V9.Api.Models.CustomerSummary
{
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "ResponseBatch", Namespace = "")]
    public class CustomerSummaryResponse
    {
        public Response Response { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string ConsumerKey { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Language { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string DateTime { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string SerialID { get; set; }
    }

    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "Response", Namespace = "")]
    public class Response
    {
        [System.Xml.Serialization.XmlElementAttribute]
        public ARSummary ARSummary { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string RequestID { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Company { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string SerialID { get; set; }
    }

    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "")]
    public partial class ARSummary
    {
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public int AgeDaysPeriod1 { get; set; }

        public int AgeDaysPeriod2 { get; set; }

        public int AgeDaysPeriod3 { get; set; }

        public int AgeDaysPeriod4 { get; set; }

        public decimal AmountDue { get; set; }

        public decimal BillingPeriodAmount { get; set; }

        public string City { get; set; }

        public string CustomerName { get; set; }

        public string DateOfFirstSale { get; set; }

        public string DateOfLastPayment { get; set; }

        public string DateOfLastSale { get; set; }

        public decimal FutureAmount { get; set; }

        public decimal OpenOrderAmount { get; set; }

        public decimal SalesLastYearYearToDate { get; set; }

        public decimal SalesMonthToDate { get; set; }

        public decimal SalesYearToDate { get; set; }

        public string TermsDescription { get; set; }

        public decimal TradeAgePeriod1Amount { get; set; }

        public decimal TradeAgePeriod2Amount { get; set; }

        public decimal TradeAgePeriod3Amount { get; set; }

        public decimal TradeAgePeriod4Amount { get; set; }

        public decimal TradeAmountDue { get; set; }

        public decimal TradeBillingPeriodAmount { get; set; }

        public decimal TradeFutureAmount { get; set; }

        public string ZipCode { get; set; }

        public int AvgDaysToPay1 { get; set; }

        public int AvgDaysToPay1Wgt { get; set; }

        public int AvgDaysToPay2 { get; set; }

        public int AvgDaysToPay2Wgt { get; set; }

        public int AvgDaysToPay3 { get; set; }

        public int AvgDaysToPay3Wgt { get; set; }

        public string AvgDaysToPayDesc1 { get; set; }

        public string AvgDaysToPayDesc2 { get; set; }

        public string AvgDaysToPayDesc3 { get; set; }

        public string CreditCheckType { get; set; }

        public decimal CreditLimit { get; set; }

        public string CustomerNum { get; set; }

        public decimal HighBalance { get; set; }

        public decimal LastPayAmount { get; set; }

        public int NumInvPastDue { get; set; }

        public int NumOpenInv { get; set; }

        public int NumPayments1 { get; set; }

        public int NumPayments2 { get; set; }

        public int NumPayments3 { get; set; }

        public string State { get; set; }

        public string TermsCode { get; set; }

        public string TradeAgePeriod1Text { get; set; }

        public string TradeAgePeriod2Text { get; set; }

        public string TradeAgePeriod3Text { get; set; }

        public string TradeAgePeriod4Text { get; set; }

        public string TradeBillingPeriodText { get; set; }
    }
}