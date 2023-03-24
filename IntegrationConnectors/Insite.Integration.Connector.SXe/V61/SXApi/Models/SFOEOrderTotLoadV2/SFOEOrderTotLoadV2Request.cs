﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

using System.Collections.Generic;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "Nxtrend.WS")]
public class SFOEOrderTotLoadV2Request
{
    public List<SFOEOrderTotLoadV2inputCreditcard> arrayCreditcard { get; set; } = new List<SFOEOrderTotLoadV2inputCreditcard>();

    public List<SFOEOrderTotLoadV2inputInheader> arrayInheader { get; set; } = new List<SFOEOrderTotLoadV2inputInheader>();

    public List<SFOEOrderTotLoadV2inputInline> arrayInline { get; set; } = new List<SFOEOrderTotLoadV2inputInline>();

    public List<SFOEOrderTotLoadV2inputHeaderextradata> arrayHeaderextradata { get; set; } = new List<SFOEOrderTotLoadV2inputHeaderextradata>();

    public List<SFOEOrderTotLoadV2inputLineextradata> arrayLineextradata { get; set; } = new List<SFOEOrderTotLoadV2inputLineextradata>();
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "SFOEOrderTotLoadV2.input.Creditcard", Namespace = "Nxtrend.WS")]
public class SFOEOrderTotLoadV2inputCreditcard
{
    public string customerID { get; set; }

    public string creditCardNumber { get; set; }

    public string paymentType { get; set; }

    public string creditCardExpiration { get; set; }

    public string cardHolder { get; set; }

    public string cvv2 { get; set; }

    public string address1 { get; set; }

    public string address2 { get; set; }

    public string address3 { get; set; }

    public string address4 { get; set; }

    public string city { get; set; }

    public string state { get; set; }

    public string zipCode { get; set; }

    public string country { get; set; }

    public string poNumber { get; set; }

    public string shipToZipCode { get; set; }

    public decimal taxAmount { get; set; }

    public decimal authorizationAmount { get; set; }
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "SFOEOrderTotLoadV2.input.Inheader", Namespace = "Nxtrend.WS")]
public class SFOEOrderTotLoadV2inputInheader
{
    public decimal taxAmount { get; set; }

    public decimal authorizationAmount { get; set; }

    public string customerID { get; set; }

    public string creditCardNumber { get; set; }

    public string paymentType { get; set; }

    public string creditCardExpiration { get; set; }

    public string warehouseID { get; set; }

    public string orderSource { get; set; }

    public string reviewOrderHold { get; set; }

    public string poNumber { get; set; }

    public string ordNumber { get; set; }

    public string workStation { get; set; }

    public string billToContact { get; set; }

    public string billToCity { get; set; }

    public string billToState { get; set; }

    public string billToZipCode { get; set; }

    public string billToPhone { get; set; }

    public string billToPhoneExt { get; set; }

    public string carrierCode { get; set; }

    public string customerAddress1 { get; set; }

    public string customerAddress2 { get; set; }

    public string customerAddress3 { get; set; }

    public string customerAddress4 { get; set; }

    public string contractNumber { get; set; }

    public string customerName { get; set; }

    public string customerCountry { get; set; }

    public string taxExemptCentury { get; set; }

    public string taxExemptDate { get; set; }

    public string taxExCertNumber { get; set; }

    public string fobCode { get; set; }

    public string requestedShipDate { get; set; }

    public string shipToAddress1 { get; set; }

    public string shipToAddress2 { get; set; }

    public string shipToAddress3 { get; set; }

    public string shipToAddress4 { get; set; }

    public string shipToContact { get; set; }

    public string shipToCity { get; set; }

    public string shipToCountry { get; set; }

    public string shipToName { get; set; }

    public string shipToNumber { get; set; }

    public string shipToState { get; set; }

    public string shipToPhone { get; set; }

    public string shipToPhoneExt { get; set; }

    public string shipToZipCode { get; set; }

    public string webTransactionType { get; set; }

    public string webProcessID { get; set; }

    public string webTransactionID { get; set; }

    public string webOrderID { get; set; }

    public string freightMethod { get; set; }

    public string orderType { get; set; }

    public string quoteReviewDate { get; set; }
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "SFOEOrderTotLoadV2.input.Inline", Namespace = "Nxtrend.WS")]
public class SFOEOrderTotLoadV2inputInline
{
    public string itemNumber { get; set; }

    public decimal orderQty { get; set; }

    public string unitOfMeasure { get; set; }

    public string warehouseID { get; set; }

    public string lineItemType { get; set; }

    public string itemDescription1 { get; set; }

    public string itemDesciption2 { get; set; }

    public decimal actualSellPrice { get; set; }

    public decimal cost { get; set; }

    public string nonStockFlag { get; set; }

    public string chargeType { get; set; }

    public string dropShip { get; set; }

    public string dueDate { get; set; }

    public decimal extendedWeight { get; set; }

    public string listPrice { get; set; }

    public int itemID { get; set; }

    public int sequenceNumber { get; set; }

    public string shipInstructionType { get; set; }
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "SFOEOrderTotLoadV2.input.Headerextradata", Namespace = "Nxtrend.WS")]
public class SFOEOrderTotLoadV2inputHeaderextradata
{
    public string fieldName { get; set; }

    public string fieldValue { get; set; }

    public int sequenceNumber { get; set; }
}

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "SFOEOrderTotLoadV2.input.Lineextradata", Namespace = "Nxtrend.WS")]
public class SFOEOrderTotLoadV2inputLineextradata
{
    public string fieldName { get; set; }

    public string fieldValue { get; set; }

    public int lineIdentifier { get; set; }

    public int sequenceNumber { get; set; }
}