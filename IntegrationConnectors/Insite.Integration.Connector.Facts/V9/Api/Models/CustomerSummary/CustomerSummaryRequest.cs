﻿// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.Facts.V9.Api.Models.CustomerSummary
{
    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "RequestBatch", Namespace = "")]
    public class CustomerSummaryRequest : IRequest
    {
        public Request Request { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string ConsumerKey { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string DateTime { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Language { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Password { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string SerialID { get; set; }
    }

    [System.SerializableAttribute]
    [System.Xml.Serialization.XmlTypeAttribute(TypeName = "Request", Namespace = "")]
    public class Request
    {
        [System.Xml.Serialization.XmlElement("Company")]
        public string Company1 { get; set; }

        public string Customer { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string Company { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string RequestID { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute]
        public string SerialID { get; set; }
    }
}
