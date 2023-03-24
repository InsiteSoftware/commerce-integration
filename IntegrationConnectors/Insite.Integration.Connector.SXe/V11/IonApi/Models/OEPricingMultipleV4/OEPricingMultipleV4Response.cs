﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[SerializableAttribute]
public class OEPricingMultipleV4Response
{
    [JsonProperty("response")]
    public Response Response { get; set; }
}

[SerializableAttribute]
public class Response
{
    [JsonProperty("cErrorMessage")]
    public string ErrorMessage { get; set; }

    [JsonProperty("tOemultprcoutV2")]
    public PriceOutV2Collection PriceOutV2Collection { get; set; }

    [JsonProperty("tOemultprcoutbrk")]
    public PriceOutBreakCollection PriceOutBreakCollection { get; set; }
}

[SerializableAttribute]
public class PriceOutV2Collection
{
    [JsonProperty("t-oemultprcoutV2")]
    public List<PriceOutV2> PriceOutV2s { get; set; } = new List<PriceOutV2>();
}

[SerializableAttribute]
public class PriceOutV2
{
    public int Seqno { get; set; }

    public string Whse { get; set; }

    public string Prod { get; set; }

    public decimal Qtyord { get; set; }

    public decimal Stkqtyord { get; set; }

    public string Unit { get; set; }

    public decimal Unitconv { get; set; }

    public decimal Baseprice { get; set; }

    public decimal Listprice { get; set; }

    public string Priceonty { get; set; }

    public string User1 { get; set; }

    public string User2 { get; set; }

    public string User3 { get; set; }

    public string User4 { get; set; }

    public string User5 { get; set; }

    public string Errormess { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public decimal Price { get; set; }

    public decimal Discamt { get; set; }

    public string Disctype { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public decimal Extamt { get; set; }

    public decimal Extdiscount { get; set; }

    public decimal Netavail { get; set; }

    public string Vcspeccostty { get; set; }

    public string Vcprccostper { get; set; }

    public decimal Vccsunperstk { get; set; }

    public int Vcspecconv { get; set; }

    public int Vcspecrecno { get; set; }

    public int Pdrecno { get; set; }

    public bool Promofl { get; set; }

    public string Priceorigcd { get; set; }

    public string Cstpertext { get; set; }

    public decimal Qtyonorder { get; set; }

    public string Duedt { get; set; }

    public decimal Freightamount { get; set; }

    public bool Freightdiscfl { get; set; }

    public bool Qtybreakexistfl { get; set; }

    public decimal Spiffamount { get; set; }

    public bool Otherwhseinvfl { get; set; }

    public decimal Commission { get; set; }
}

[SerializableAttribute]
public class PriceOutBreakCollection
{
    [JsonProperty("t-oemultprcoutbrk")]
    public List<PriceOutBreak> PriceOutBreaks { get; set; } = new List<PriceOutBreak>();
}

[SerializableAttribute]
public class PriceOutBreak
{
    public int Seqno { get; set; }

    public string Whse { get; set; }

    public string Prod { get; set; }

    public string User1 { get; set; }

    public string User2 { get; set; }

    public string User3 { get; set; }

    public string User4 { get; set; }

    public string User5 { get; set; }

    public decimal Pricebreak1 { get; set; }

    public decimal Pricebreak2 { get; set; }

    public decimal Pricebreak3 { get; set; }

    public decimal Pricebreak4 { get; set; }

    public decimal Pricebreak5 { get; set; }

    public decimal Pricebreak6 { get; set; }

    public decimal Pricebreak7 { get; set; }

    public decimal Pricebreak8 { get; set; }

    public decimal Pricebreak9 { get; set; }

    public decimal Discountpercent1 { get; set; }

    public decimal Discountpercent2 { get; set; }

    public decimal Discountpercent3 { get; set; }

    public decimal Discountpercent4 { get; set; }

    public decimal Discountpercent5 { get; set; }

    public decimal Discountpercent6 { get; set; }

    public decimal Discountpercent7 { get; set; }

    public decimal Discountpercent8 { get; set; }

    public decimal Discountpercent9 { get; set; }

    public decimal Quantitybreak1 { get; set; }

    public decimal Quantitybreak2 { get; set; }

    public decimal Quantitybreak3 { get; set; }

    public decimal Quantitybreak4 { get; set; }

    public decimal Quantitybreak5 { get; set; }

    public decimal Quantitybreak6 { get; set; }

    public decimal Quantitybreak7 { get; set; }

    public decimal Quantitybreak8 { get; set; }
}