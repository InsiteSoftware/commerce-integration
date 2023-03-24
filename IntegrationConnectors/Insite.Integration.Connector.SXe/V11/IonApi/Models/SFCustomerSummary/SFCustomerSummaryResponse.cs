﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.SXe.V11.IonApi.Models.SFCustomerSummary;

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[SerializableAttribute]
public class SFCustomerSummaryResponse
{
    [JsonProperty("response")]
    public Response Response { get; set; }
}

[SerializableAttribute]
public class Response
{
    [JsonProperty("cErrorMessage")]
    public string ErrorMessage { get; set; }

    [JsonProperty("tCustsummary")]
    public CustomerSummaryCollection CustomerSummaryCollection { get; set; }
}

[SerializableAttribute]
public class CustomerSummaryCollection
{
    [JsonProperty("t-custsummary")]
    public List<CustomerSummary> CustomerSummaries { get; set; } = new List<CustomerSummary>();
}

[SerializableAttribute]
public class CustomerSummary
{
    public string Custname { get; set; }

    public string Addr1 { get; set; }

    public string Addr2 { get; set; }

    public string Addr3 { get; set; }

    public string Addr4 { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public string Countrycd { get; set; }

    public string Zipcd { get; set; }

    public decimal Openordamt { get; set; }

    public decimal Billprdamt { get; set; }

    public decimal Ageprd1 { get; set; }

    public decimal Ageprd2 { get; set; }

    public decimal Ageprd3 { get; set; }

    public decimal Ageprd4 { get; set; }

    public decimal Amtdue { get; set; }

    public decimal Futureamt { get; set; }

    public decimal SalesMTD { get; set; }

    public decimal SalesYTD { get; set; }

    public decimal SalesLYTD { get; set; }

    public DateTime? Lastpaydt { get; set; }

    public DateTime? Firstsaledt { get; set; }

    public DateTime? Lastsaledt { get; set; }

    public string Currencycd { get; set; }

    public decimal Tradeopenordamt { get; set; }

    public decimal Tradebillprdamt { get; set; }

    public decimal Tradeageprd1 { get; set; }

    public decimal Tradeageprd2 { get; set; }

    public decimal Tradeageprd3 { get; set; }

    public decimal Tradeageprd4 { get; set; }

    public decimal Tradeamtdue { get; set; }

    public decimal Tradefutureamt { get; set; }

    public decimal TradesalesMTD { get; set; }

    public decimal TradesalesYTD { get; set; }

    public decimal TradesalesLYTD { get; set; }

    public string Tradecurrencycd { get; set; }

    public string Termsdesc { get; set; }

    public decimal Agedaysper1 { get; set; }

    public decimal Agedaysper2 { get; set; }

    public decimal Agedaysper3 { get; set; }

    public decimal Agedaysper4 { get; set; }
}