﻿
// <auto-generated>
// This isn't auto-generated, but it keeps stylecop from complaining
// </auto-generated>

namespace Insite.Integration.Connector.SXe.V11.IonApi.Models.ARCustomerMnt;

using Newtonsoft.Json;

[System.SerializableAttribute]
public class ARCustomerMntResponse
{
    [JsonProperty("response")]
    public Response Response { get; set; }
}

[System.SerializableAttribute]
public class Response
{
    [JsonProperty("cErrorMessage")]
    public string ErrorMessage { get; set; }

    public string ReturnData { get; set; }
}