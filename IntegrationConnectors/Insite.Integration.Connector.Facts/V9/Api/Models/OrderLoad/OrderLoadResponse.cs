﻿// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad
{
    using System.Collections.Generic;

    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "ResponseBatch", Namespace = "")]
    public class OrderLoadResponse
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
        public string CompletionCode { get; set; }

        public string Message { get; set; }

        public string OrderNumber { get; set; }

        public decimal InvoiceAmount { get; set; }
    }
}
