namespace Insite.Integration.Connector.SXe.V10.SXApi.Models.Shared;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(
    Namespace = "http://schemas.datacontract.org/2004/07/NxT_API.com.infor.sxapi.connection"
)]
public class CallConnection
{
    public int CompanyNumber { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string ConnectionString { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string DomainIdentifier { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string OperatorInitials { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string OperatorPassword { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public bool? StateFreeAppserver { get; set; }
}
