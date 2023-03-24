﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;

using System.Collections.Generic;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.datacontract.org/2004/07/NxT_API.com.infor.sxapi.SFOEOrderTotLoadV" + "4")]
public class SFOEOrderTotLoadV4Response
{
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string ErrorMessage { get; set; }

    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    public List<Outoutextamt1> Outoutextamt { get; set; } = new List<Outoutextamt1>();

    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    public List<Outoutfieldvalue4> Outoutfieldvalue { get; set; } = new List<Outoutfieldvalue4>();

    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    public List<Outouthdrfreight> Outouthdrfreight { get; set; } = new List<Outouthdrfreight>();

    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    public List<Outoutheader3> Outoutheader { get; set; } = new List<Outoutheader3>();

    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    public List<Outoutline3> Outoutline { get; set; } = new List<Outoutline3>();

    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    public List<Outouttotal3> Outouttotal { get; set; } = new List<Outouttotal3>();
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "Outoutextamt", Namespace = "http://schemas.datacontract.org/2004/07/NxT_API.com.infor.sxapi.SFOEOrderTotLoadV" + "4")]
public class Outoutextamt1
{
    public decimal Amount { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string Description { get; set; }

    public int SequenceNumber { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string Type { get; set; }
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "Outoutfieldvalue", Namespace = "http://schemas.datacontract.org/2004/07/NxT_API.com.infor.sxapi.SFOEOrderTotLoadV" + "4")]
public class Outoutfieldvalue4
{
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string FieldName { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string FieldValue { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string Level { get; set; }

    public int LineNumber { get; set; }

    public int SequenceNumber { get; set; }
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://schemas.datacontract.org/2004/07/NxT_API.com.infor.sxapi.SFOEOrderTotLoadV" + "4")]
public class Outouthdrfreight
{
    public decimal AddonWithoutFreight { get; set; }

    public decimal CalulcatedOrderFreight { get; set; }

    public int CompanyNumber { get; set; }

    public decimal OrderFreight { get; set; }

    public decimal OrderFreightExtra1 { get; set; }

    public decimal OrderFreightExtra2 { get; set; }

    public decimal OrderWeight { get; set; }

    public decimal OrderWeightLimit { get; set; }

    public decimal OverMaxRate { get; set; }

    public decimal ShipFreight { get; set; }

    public decimal ShipFreightExtra1 { get; set; }

    public decimal ShipFreightExtra2 { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string ShipVia { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string ShipViaDescription { get; set; }

    public decimal ShipWeight { get; set; }

    public decimal ShipWeightLimit { get; set; }

    public decimal TotalInvoiceAmount { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string Warehouse { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string Zone { get; set; }
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "Outoutheader", Namespace = "http://schemas.datacontract.org/2004/07/NxT_API.com.infor.sxapi.SFOEOrderTotLoadV" + "4")]
public class Outoutheader3
{
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string CompletionCode { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string CustPONumber { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string MessageOut { get; set; }

    public int OrderNumber { get; set; }

    public int OrderSuffix { get; set; }

    public decimal TotalInvoiceAmount { get; set; }
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "Outoutline", Namespace = "http://schemas.datacontract.org/2004/07/NxT_API.com.infor.sxapi.SFOEOrderTotLoadV" + "4")]
public class Outoutline3
{
    public decimal ActualSellPrice { get; set; }

    public decimal BackorderQty { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string Description { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string ItemNumber { get; set; }

    public decimal LineAmount { get; set; }

    public int LineNumber { get; set; }

    public int OrderNumber { get; set; }

    public decimal OrderQty { get; set; }

    public int OrderSuffix { get; set; }

    public decimal ShipQty { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string UnitOfMeasure { get; set; }
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "Outouttotal", Namespace = "http://schemas.datacontract.org/2004/07/NxT_API.com.infor.sxapi.SFOEOrderTotLoadV" + "4")]
public class Outouttotal3
{
    public decimal AmountAuthorized { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string CompletionCode { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string CurrencyCode { get; set; }

    public decimal FederalExciseAmount { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string MessageOut { get; set; }

    public decimal SalesAmount { get; set; }

    public decimal SalesTaxAmount { get; set; }

    public decimal TotalContainerAmount { get; set; }

    public decimal TotalInvoiceAmount { get; set; }

    public decimal TotalOrderValue { get; set; }

    public decimal TotalOrderWeight { get; set; }

    public decimal TotalSpecialCharge { get; set; }

    public decimal TradeDiscountAmount { get; set; }
}