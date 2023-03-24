﻿// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.OePricingMultipleV4
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class OePricingMultipleV4Request
    {
        [JsonProperty("request")]
        public Request Request { get; set; }
    }

    public class Request
    {
        public int CompanyNumber { get; set; }

        public string OperatorInit { get; set; }

        public string OperatorPassword { get; set; }

        public string CustomerNumber { get; set; }

        public string EdiPartnerCode { get; set; }

        public string ShipTo { get; set; }

        public string KeyCode { get; set; }

        public bool? GetPriceBreaks { get; set; }

        public bool? UseDefaultWhse { get; set; }

        public bool? SendFullQtyOnOrder { get; set; }

        public bool? CheckOtherWhseInventory { get; set; }

        public string PricingMethod { get; set; }

        public string ExtraData { get; set; }

        [JsonProperty("tOemultprcinV2")]
        public PriceInV2Collection PriceInV2Collection { get; set; } = new PriceInV2Collection();
    }

    public class PriceInV2Collection
    {
        [JsonProperty("t-oemultprcinV2")]
        public List<PriceInV2> PriceInV2s { get; set; } = new List<PriceInV2>();
    }

    public class PriceInV2
    {
        [JsonProperty("Seqno")]
        public int? SeqNo { get; set; }

        public string Whse { get; set; }

        public string Prod { get; set; }

        [JsonProperty("Operchannel")]
        public string OperChannel { get; set; }

        [JsonProperty("Qtyord")]
        public decimal? QtyOrd { get; set; }

        public string Unit { get; set; }

        [JsonProperty("Extradata")]
        public string ExtraData { get; set; }

        public string User1 { get; set; }

        public string User2 { get; set; }

        public string User3 { get; set; }

        public string User4 { get; set; }

        public string User5 { get; set; }
    }
}