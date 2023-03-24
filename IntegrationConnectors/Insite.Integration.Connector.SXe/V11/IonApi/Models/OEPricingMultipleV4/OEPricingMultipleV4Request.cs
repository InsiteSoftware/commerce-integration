﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4;

using System.Collections.Generic;
using Newtonsoft.Json;

[System.SerializableAttribute]
public class OEPricingMultipleV4Request
{
    [JsonProperty("request")]
    public Request Request { get; set; }
}

[System.SerializableAttribute]
public class Request : IRequest
{
    public int CompanyNumber { get; set; }

    public string OperatorInit { get; set; }

    public string OperatorPassword { get; set; }

    public long CustomerNumber { get; set; }

    public string EdiPartnerCode { get; set; }

    public string ShipTo { get; set; }

    public string KeyCode { get; set; }

    public bool GetPriceBreaks { get; set; }

    public bool UseDefaultWhse { get; set; }

    public bool SendFullQtyOnOrder { get; set; }

    public bool CheckOtherWhseInventory { get; set; }

    public string PricingMethod { get; set; }

    public string ExtraData { get; set; }

    [JsonProperty("tOemultprcinV2")]
    public PriceInV2Collection PriceInV2Collection { get; set; } = new PriceInV2Collection();
}

[System.SerializableAttribute]
public class PriceInV2Collection
{
    [JsonProperty("t-oemultprcinV2")]
    public List<PriceInV2> PriceInV2s { get; set; } = new List<PriceInV2>();
}

[System.SerializableAttribute]
public class PriceInV2
{
    public int Seqno { get; set; }

    public string Whse { get; set; }

    public string Prod { get; set; }

    public string Operchannel { get; set; }

    public decimal Qtyord { get; set; }

    public string Unit { get; set; }

    public string Extradata { get; set; }

    public string User1 { get; set; }

    public string User2 { get; set; }

    public string User3 { get; set; }

    public string User4 { get; set; }

    public string User5 { get; set; }
}