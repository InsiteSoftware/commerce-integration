namespace Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.Entities;

using System;

[SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "Token", Namespace = "")]
public class Token
{
    public string AccessToken { get; set; }

    public string TokenType { get; set; }

    public int? ExpiresIn { get; set; }

    public string RefreshToken { get; set; }

    public string Scope { get; set; }

    public Guid? SessionId { get; set; }

    public string ConsumerUid { get; set; }
}
