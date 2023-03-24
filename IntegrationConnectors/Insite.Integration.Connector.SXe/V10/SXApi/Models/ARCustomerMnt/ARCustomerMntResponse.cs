namespace Insite.Integration.Connector.SXe.V10.SXApi.Models.ARCustomerMnt;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(
    Namespace = "http://schemas.datacontract.org/2004/07/NxT_API.com.infor.sxapi.ARCustomerMnt"
)]
public class ARCustomerMntResponse
{
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string ErrorMessage { get; set; }

    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string ReturnData { get; set; }
}
