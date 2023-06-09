﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;

using System.Collections.Generic;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://types.salesparthandling.webservices.ifsworld.com/")]
public class customerPriceResponse
{
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string site { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string customerNo { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string currencyCode { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string errorText { get; set; }

    [System.Xml.Serialization.XmlElementAttribute("parts", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<salesPartPriceResData> parts { get; set; } = new List<salesPartPriceResData>();
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://types.salesparthandling.webservices.ifsworld.com/")]
public class salesPartPriceResData
{
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string productNo { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public int quantity { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal saleUnitPrice { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal saleUnitPriceIncVat { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal currencyRate { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal discount { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal saleNetPrice { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string priceSource { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string priceSourceId { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string priceSourceDescription { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public int minimumQty { get; set; }
}