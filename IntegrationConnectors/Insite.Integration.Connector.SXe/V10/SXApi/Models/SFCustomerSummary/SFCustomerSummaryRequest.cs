namespace Insite.Integration.Connector.SXe.V10.SXApi.Models.SFCustomerSummary;

[System.SerializableAttribute]
[System.Xml.Serialization.XmlTypeAttribute(
    Namespace = "http://schemas.datacontract.org/2004/07/NxT_API.com.infor.sxapi.SFCustomerSummary"
        + ""
)]
public class SFCustomerSummaryRequest
{
    public decimal CustomerNumber { get; set; }
}
