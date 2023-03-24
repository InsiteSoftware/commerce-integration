﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;

using System.Collections.Generic;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://types.salesparthandling.webservices.ifsworld.com/")]
public class customerPriceRequest
{
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string site { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string customerNo { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string currencyCode { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string agreementId { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public System.DateTime effectivityDate { get; set; }

    [System.Xml.Serialization.XmlIgnoreAttribute]
    public bool effectivityDateSpecified { get; set; }

    [System.Xml.Serialization.XmlElementAttribute("parts", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<salesPartPriceReqData> parts { get; set; } = new List<salesPartPriceReqData>();
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://types.salesparthandling.webservices.ifsworld.com/")]
public class salesPartPriceReqData
{
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string productNo { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public int quantity { get; set; }

    [System.Xml.Serialization.XmlIgnoreAttribute]
    public bool quantitySpecified { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string priceListNo { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string conditionCode { get; set; }
}