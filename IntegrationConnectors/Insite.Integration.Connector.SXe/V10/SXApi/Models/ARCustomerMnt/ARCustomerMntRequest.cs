﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.SXe.V10.SXApi.Models.ARCustomerMnt;

using System.Collections.Generic;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.datacontract.org/2004/07/NxT_API.com.infor.sxapi.ARCustomerMnt")]
public class ARCustomerMntRequest
{
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string ExtraData { get; set; }

    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    public List<InfieldModification> InfieldModification { get; set; } = new List<InfieldModification>();
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.datacontract.org/2004/07/NxT_API.com.infor.sxapi.ARCustomerMnt")]
public class InfieldModification
{
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string FieldName { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string FieldValue { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string Key1 { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string Key2 { get; set; }

    public int SequenceNumber { get; set; }

    public int SetNumber { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string UpdateMode { get; set; }
}