namespace Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.AccountsReceivableSummary;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "Request", Namespace = "")]
public class AccountsReceivableSummaryRequest
{
    public string CompanyNumber { get; set; }

    public string CustomerNumber { get; set; }

    [System.Xml.Serialization.XmlAttributeAttribute]
    public string Name { get; set; }
}
