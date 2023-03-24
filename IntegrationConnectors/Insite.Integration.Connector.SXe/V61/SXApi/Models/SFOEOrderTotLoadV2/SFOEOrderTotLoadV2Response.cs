﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

using System.Collections.Generic;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public class SFOEOrderTotLoadV2Response
{
    public string errorMessage { get; set; }

    public List<SFOEOrderTotLoadV2outputOutheader> arrayOutheader { get; set; } = new List<SFOEOrderTotLoadV2outputOutheader>();

    public List<SFOEOrderTotLoadV2outputOutline> arrayOutline { get; set; } = new List<SFOEOrderTotLoadV2outputOutline>();

    public List<SFOEOrderTotLoadV2outputOuttotal> arrayOuttotal { get; set; } = new List<SFOEOrderTotLoadV2outputOuttotal>();
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "SFOEOrderTotLoadV2.output.Outheader", Namespace = "Nxtrend.WS")]
public class SFOEOrderTotLoadV2outputOutheader
{
    public string messageOut { get; set; }

    public int orderNumber { get; set; }

    public int orderSuffix { get; set; }

    public string completionCode { get; set; }

    public string custPONumber { get; set; }

    public decimal totalInvoiceAmount { get; set; }
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "SFOEOrderTotLoadV2.output.Outline", Namespace = "Nxtrend.WS")]
public class SFOEOrderTotLoadV2outputOutline
{
    public int orderNumber { get; set; }

    public int orderSuffix { get; set; }

    public int lineNumber { get; set; }

    public string itemNumber { get; set; }

    public string description { get; set; }

    public decimal orderQty { get; set; }

    public decimal shipQty { get; set; }

    public decimal backorderQty { get; set; }

    public decimal actualSellPrice { get; set; }

    public decimal lineAmount { get; set; }

    public string unitOfMeasure { get; set; }
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "SFOEOrderTotLoadV2.output.Outtotal", Namespace = "Nxtrend.WS")]
public class SFOEOrderTotLoadV2outputOuttotal
{
    public string messageOut { get; set; }

    public decimal salesAmount { get; set; }

    public decimal totalSpecialCharge { get; set; }

    public decimal tradeDiscountAmount { get; set; }

    public decimal salesTaxAmount { get; set; }

    public decimal federalExciseAmount { get; set; }

    public decimal totalContainerAmount { get; set; }

    public decimal totalOrderValue { get; set; }

    public decimal totalInvoiceAmount { get; set; }

    public string currencyCode { get; set; }

    public decimal amountAuthorized { get; set; }

    public string completionCode { get; set; }

    public decimal totalOrderWeight { get; set; }
}