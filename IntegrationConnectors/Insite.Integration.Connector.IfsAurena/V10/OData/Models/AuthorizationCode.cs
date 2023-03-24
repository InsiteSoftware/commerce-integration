namespace Insite.Integration.Connector.IfsAurena.V10.OData.Models;

using System;
using Newtonsoft.Json;

public class AuthorizationCode
{
    public string OrderNo { get; set; }

    public string ReferenceId { get; set; }

    public string AuthCode { get; set; }
}
