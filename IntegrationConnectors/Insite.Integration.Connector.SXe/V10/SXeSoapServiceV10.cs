namespace Insite.Integration.Connector.SXe.V10;

using Insite.Integration.Connector.SXe.V10.SXApi.Models.ARCustomerMnt;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.OEPricingMultipleV4;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.Shared;

[System.Web.Services.WebServiceBindingAttribute]
internal sealed class SXeSoapServiceV10 : System.Web.Services.Protocols.SoapHttpClientProtocol
{
    public SXeSoapServiceV10(string url)
    {
        this.Url = url;
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute(
        "http://tempuri.org/IServiceAR/ARCustomerMnt",
        RequestNamespace = "http://tempuri.org/",
        ResponseNamespace = "http://tempuri.org/",
        Use = System.Web.Services.Description.SoapBindingUse.Literal,
        ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped
    )]
    public ARCustomerMntResponse ARCustomerMnt(
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            CallConnection callConnection,
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            ARCustomerMntRequest request
    )
    {
        var results = this.Invoke("ARCustomerMnt", new object[] { callConnection, request });
        return (ARCustomerMntResponse)results[0];
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute(
        "http://tempuri.org/IServiceOE/OEPricingMultipleV4",
        RequestNamespace = "http://tempuri.org/",
        ResponseNamespace = "http://tempuri.org/",
        Use = System.Web.Services.Description.SoapBindingUse.Literal,
        ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped
    )]
    public OEPricingMultipleV4Response OEPricingMultipleV4(
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            CallConnection callConnection,
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            OEPricingMultipleV4Request request
    )
    {
        var results = this.Invoke("OEPricingMultipleV4", new object[] { callConnection, request });
        return (OEPricingMultipleV4Response)results[0];
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute(
        "http://tempuri.org/IServiceSF/SFCustomerSummary",
        RequestNamespace = "http://tempuri.org/",
        ResponseNamespace = "http://tempuri.org/",
        Use = System.Web.Services.Description.SoapBindingUse.Literal,
        ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped
    )]
    public SFCustomerSummaryResponse SFCustomerSummary(
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            CallConnection callConnection,
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            SFCustomerSummaryRequest request
    )
    {
        var results = this.Invoke("SFCustomerSummary", new object[] { callConnection, request });
        return (SFCustomerSummaryResponse)results[0];
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute(
        "http://tempuri.org/IServiceSF/SFOEOrderTotLoadV4",
        RequestNamespace = "http://tempuri.org/",
        ResponseNamespace = "http://tempuri.org/",
        Use = System.Web.Services.Description.SoapBindingUse.Literal,
        ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped
    )]
    public SFOEOrderTotLoadV4Response SFOEOrderTotLoadV4(
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            CallConnection callConnection,
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
            SFOEOrderTotLoadV4Request request
    )
    {
        var results = this.Invoke("SFOEOrderTotLoadV4", new object[] { callConnection, request });
        return (SFOEOrderTotLoadV4Response)results[0];
    }
}
