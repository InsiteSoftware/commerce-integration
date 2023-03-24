﻿// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability
{
    using System.Collections.Generic;

    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "ResponseBatch", Namespace = "")]
    public class PriceAvailabilityResponse
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
        public List<ResponseItem> Items { get; set; } = new List<ResponseItem>();

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string RequestID { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Company { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string SerialID { get; set; }
    }

    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "Item", Namespace = "")]
    public class ResponseItem
    {
        public string ItemNumber { get; set; }

        public decimal QuantityAvailable { get; set; }

        public bool NonStockFlag { get; set; }

        public decimal OrderQuantity { get; set; }

        public string UnitOfMeasure { get; set; }

        public string WarehouseID { get; set; }

        public decimal Price { get; set; }

        public string PricingUnitOfMeasure { get; set; }

        public decimal PricingUnitPrice { get; set; }

        public decimal PricingUnitStdPrice { get; set; }

        public decimal PricingUnitL1Price { get; set; }

        public decimal PricingUnitL2Price { get; set; }

        public decimal PricingUnitL3Price { get; set; }

        public decimal PricingUnitL4Price { get; set; }

        public decimal PricingUnitL5Price { get; set; }

        public decimal PricingUnitL6Price { get; set; }

        public decimal ExtendedPrice { get; set; }
        
        public string NextPODate { get; set; }

        public decimal NextPOQuantity { get; set; }

        public string ErrorMessage { get; set; }
    }
}
