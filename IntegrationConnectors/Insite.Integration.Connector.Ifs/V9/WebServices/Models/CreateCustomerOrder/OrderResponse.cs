﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;

using System.Collections.Generic;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://types.customerorderhandling.webservices.ifsworld.com/")]
public class orderResponse
{
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string orderNo { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string contract { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string info { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string errorMessage { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public bool orderCreated { get; set; }

    [System.Xml.Serialization.XmlElementAttribute("lines", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<orderLine> lines { get; set; } = new List<orderLine>();
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://types.customerorderhandling.webservices.ifsworld.com/")]
public class orderLine
{
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string orderNo { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string lineNo { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string catalogNo { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public decimal buyQtyDue { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public System.DateTime plannedDeliveryDate { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string info { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string errorMessage { get; set; }
}