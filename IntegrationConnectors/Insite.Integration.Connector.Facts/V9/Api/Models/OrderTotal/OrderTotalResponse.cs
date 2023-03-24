﻿// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.Facts.V9.Api.Models.OrderTotal
{
    using System.Collections.Generic;

    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "ResponseBatch", Namespace = "")]
    public class OrderTotalResponse
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
        [System.Xml.Serialization.XmlArrayAttribute]
        public List<ResponseOrder> Orders { get; set; } = new List<ResponseOrder>();

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string RequestID { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Company { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string SerialID { get; set; }
    }

    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "Order", Namespace = "")]
    public class ResponseOrder
    {
        public string CarrierCode { get; set; }

        public decimal SalesAmount { get; set; }

        public decimal SalesTaxAmount { get; set; }

        public decimal TotalInvoiceAmount { get; set; }

        public decimal TotalOrderValue { get; set; }

        public decimal TotalOrderWeight { get; set; }

        public decimal TotalSpecialCharge { get; set; }

        public decimal TradeDiscountAmount { get; set; }

        [System.Xml.Serialization.XmlArrayAttribute]
        public List<AdditionalCharge> AdditionalCharges { get; set; } = new List<AdditionalCharge>();
    }

    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "AdditionalCharge", Namespace = "")]
    public class AdditionalCharge
    {
        public decimal ChargeAmount { get; set; }

        public string ChargeDescription { get; set; }
    }
}
