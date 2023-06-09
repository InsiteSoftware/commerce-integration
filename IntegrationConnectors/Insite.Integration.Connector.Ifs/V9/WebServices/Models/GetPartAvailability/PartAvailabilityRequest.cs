﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;

using System.Collections.Generic;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://types.salesparthandling.webservices.ifsworld.com/")]
public class partAvailabilityRequest
{
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string site { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string customerNo { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string addressId { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string custOwnAddressId { get; set; }

    [System.Xml.Serialization.XmlElementAttribute("partsAvailabile", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public List<partAvailabilityReqData> partsAvailabile { get; set; } = new List<partAvailabilityReqData>();
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://types.salesparthandling.webservices.ifsworld.com/")]
public class partAvailabilityReqData
{
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string productNo { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public int wantedQuantity { get; set; }

    [System.Xml.Serialization.XmlIgnoreAttribute]
    public bool wantedQuantitySpecified { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public System.DateTime wantedDeliveryDate { get; set; }

    [System.Xml.Serialization.XmlIgnoreAttribute]
    public bool wantedDeliveryDateSpecified { get; set; }
}