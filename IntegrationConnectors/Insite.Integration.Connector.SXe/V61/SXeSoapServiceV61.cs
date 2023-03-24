namespace Insite.Integration.Connector.SXe.V61;

using Insite.Integration.Connector.SXe.V61.SXApi.Models.ARCustomerMnt;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

[System.Web.Services.WebServiceBindingAttribute]
internal sealed class SXeSoapServiceV61 : System.Web.Services.Protocols.SoapHttpClientProtocol
{
    public SXeSoapServiceV61(string url)
    {
        this.Url = url;
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute(
        "Nxtrend.WS/ARCustomerMnt",
        RequestNamespace = "Nxtrend.WS",
        ResponseNamespace = "Nxtrend.WS",
        Use = System.Web.Services.Description.SoapBindingUse.Literal,
        ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped
    )]
    public ARCustomerMntResponse ARCustomerMnt(
        string connectString,
        string userCode,
        string password,
        ARCustomerMntRequest requestObject
    )
    {
        var parameters = new object[] { connectString, userCode, password, requestObject };
        var results = this.Invoke("ARCustomerMnt", parameters);
        return (ARCustomerMntResponse)results[0];
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute(
        "Nxtrend.WS/OEPricingMultipleV3",
        RequestNamespace = "Nxtrend.WS",
        ResponseNamespace = "Nxtrend.WS",
        Use = System.Web.Services.Description.SoapBindingUse.Literal,
        ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped
    )]
    public OEPricingMultipleV3Response OEPricingMultipleV3(
        string connectString,
        string userCode,
        string password,
        OEPricingMultipleV3Request requestObject
    )
    {
        var parameters = new object[] { connectString, userCode, password, requestObject };
        var results = this.Invoke("OEPricingMultipleV3", parameters);
        return (OEPricingMultipleV3Response)results[0];
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute(
        "Nxtrend.WS/SFCustomerSummary",
        RequestNamespace = "Nxtrend.WS",
        ResponseNamespace = "Nxtrend.WS",
        Use = System.Web.Services.Description.SoapBindingUse.Literal,
        ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped
    )]
    public SFCustomerSummaryResponse SFCustomerSummary(
        string connectString,
        string userCode,
        string password,
        SFCustomerSummaryRequest requestObject
    )
    {
        var parameters = new object[] { connectString, userCode, password, requestObject };
        var results = this.Invoke("SFCustomerSummary", parameters);
        return (SFCustomerSummaryResponse)results[0];
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute(
        "Nxtrend.WS/SFOEOrderTotLoadV2",
        RequestNamespace = "Nxtrend.WS",
        ResponseNamespace = "Nxtrend.WS",
        Use = System.Web.Services.Description.SoapBindingUse.Literal,
        ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped
    )]
    public SFOEOrderTotLoadV2Response SFOEOrderTotLoadV2(
        string connectString,
        string userCode,
        string password,
        SFOEOrderTotLoadV2Request requestObject
    )
    {
        var parameters = new object[] { connectString, userCode, password, requestObject };
        var results = this.Invoke("SFOEOrderTotLoadV2", parameters);
        return (SFOEOrderTotLoadV2Response)results[0];
    }
}
