namespace Insite.Integration.Connector.Ifs.Services;

using System;
using System.Text;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;

[System.Web.Services.WebServiceBindingAttribute]
internal sealed class IfsSoapService : System.Web.Services.Protocols.SoapHttpClientProtocol
{
    public string UserName { get; set; }

    public string Password { get; set; }

    public IfsSoapService(string url, string userName, string password)
    {
        this.Url = url;
        this.UserName = userName;
        this.Password = password;
    }

    protected override System.Net.WebRequest GetWebRequest(Uri uri)
    {
        var request = base.GetWebRequest(uri);
        var credentialBuffer = new UTF8Encoding().GetBytes(this.UserName + ":" + this.Password);
        request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(credentialBuffer);
        return request;
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute(
        "",
        RequestNamespace = "http://customerorderservices.customerorderhandling.webservices.ifsworld.com/",
        ResponseNamespace = "http://customerorderservices.customerorderhandling.webservices.ifsworld.com/",
        Use = System.Web.Services.Description.SoapBindingUse.Literal,
        ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped
    )]
    [return: System.Xml.Serialization.XmlElementAttribute(
        "return",
        Form = System.Xml.Schema.XmlSchemaForm.Unqualified
    )]
#pragma warning disable SA1300 // Element must begin with upper-case letter
    public orderResponse createCustomerOrder(
        [System.Xml.Serialization.XmlElementAttribute(
            Form = System.Xml.Schema.XmlSchemaForm.Unqualified
        )]
            customerOrder orderRequest
    )
#pragma warning restore SA1300 // Element must begin with upper-case letter
    {
        var results = this.Invoke("createCustomerOrder", new object[] { orderRequest });
        return (orderResponse)results[0];
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute(
        "",
        RequestNamespace = "http://salespartservices.salesparthandling.webservices.ifsworld.com/",
        ResponseNamespace = "http://salespartservices.salesparthandling.webservices.ifsworld.com/",
        Use = System.Web.Services.Description.SoapBindingUse.Literal,
        ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped
    )]
    [return: System.Xml.Serialization.XmlElementAttribute(
        "return",
        Form = System.Xml.Schema.XmlSchemaForm.Unqualified
    )]
#pragma warning disable SA1300 // Element must begin with upper-case letter
    public customerPriceResponse getCustomerPrice(
        [System.Xml.Serialization.XmlElementAttribute(
            Form = System.Xml.Schema.XmlSchemaForm.Unqualified
        )]
            customerPriceRequest customerPriceParams
    )
#pragma warning restore SA1300 // Element must begin with upper-case letter
    {
        var results = this.Invoke("getCustomerPrice", new object[] { customerPriceParams });
        return (customerPriceResponse)results[0];
    }

    [System.Web.Services.Protocols.SoapDocumentMethodAttribute(
        "",
        RequestNamespace = "http://salespartservices.salesparthandling.webservices.ifsworld.com/",
        ResponseNamespace = "http://salespartservices.salesparthandling.webservices.ifsworld.com/",
        Use = System.Web.Services.Description.SoapBindingUse.Literal,
        ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped
    )]
    [return: System.Xml.Serialization.XmlElementAttribute(
        "return",
        Form = System.Xml.Schema.XmlSchemaForm.Unqualified
    )]
#pragma warning disable SA1300 // Element must begin with upper-case letter
    public partAvailabilityResponse getPartAvailability(
        [System.Xml.Serialization.XmlElementAttribute(
            Form = System.Xml.Schema.XmlSchemaForm.Unqualified
        )]
            partAvailabilityRequest partAvailabilityParams
    )
#pragma warning restore SA1300 // Element must begin with upper-case letter
    {
        var results = this.Invoke("getPartAvailability", new object[] { partAvailabilityParams });
        return (partAvailabilityResponse)results[0];
    }
}
