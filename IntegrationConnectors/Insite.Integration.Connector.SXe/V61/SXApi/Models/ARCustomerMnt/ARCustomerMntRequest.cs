﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.SXe.V61.SXApi.Models.ARCustomerMnt;

using System.Collections.Generic;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public class ARCustomerMntRequest
{
    public List<ARCustomerMntinputFieldModification> arrayFieldModification { get; set; } = new List<ARCustomerMntinputFieldModification>();

    public string extraData { get; set; }
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "ARCustomerMnt.input.FieldModification", Namespace = "Nxtrend.WS")]
public class ARCustomerMntinputFieldModification
{
    public int setNumber { get; set; }

    public int sequenceNumber { get; set; }

    public string key1 { get; set; }

    public string key2 { get; set; }

    public string updateMode { get; set; }

    public string fieldName { get; set; }

    public string fieldValue { get; set; }
}