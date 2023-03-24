namespace Insite.Integration.Connector.SXe.Tests.V11;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Insite.Common.HttpUtilities;
using Insite.Core.Interfaces.Plugins.Caching;
using Insite.Integration.Connector.SXe.V11;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.ARCustomerMnt;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Request = Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4.Request;

[TestFixture]
internal class SXeApiServiceV11Tests
{
    [Test]
    public void OEPricingMultipleV4_Should_Deserialize_Null_Price_And_Extamt_To_Default()
    {
        var cacheManager = new Mock<ICacheManager>();
        cacheManager.Setup(o => o.Contains(SXeApiServiceV11.TokenCacheKey)).Returns(true);
        cacheManager
            .Setup(o => o.Get<string>(SXeApiServiceV11.TokenCacheKey))
            .Returns("bearerToken");
        var httpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        @"
{
    ""response"": {
        ""cErrorMessage"": """",
        ""tOemultprcoutV2"": {
            ""t-oemultprcoutV2"": [
                {
                    ""prod"": ""CBD2 SIZES: 12X12 TO 24X24"",
                    ""price"": null,
                    ""extamt"": null
                }
            ]
        },
        ""tOemultprcoutbrk"": {
            ""t-oemultprcoutbrk"": []
        }
    }
}
"
                    )
                }
            );
        var httpClient = new HttpClient(httpMessageHandler.Object);
        var httpClientProvider = new Mock<HttpClientProvider>();
        httpClientProvider
            .Setup(o => o.GetHttpClient(It.IsAny<Uri>(), It.IsAny<HttpClientHandler>()))
            .Callback<Uri, HttpClientHandler>((uri, handler) => httpClient.BaseAddress = uri)
            .Returns(httpClient);
        var apiService = new SXeApiServiceV11(
            "http://localhost",
            "http://localhost",
            "operatorInitials",
            "operatorPassword",
            123,
            "tokenClientId",
            "tokenClientSecret",
            "tokenUserName",
            "tokenPassword",
            httpClientProvider.Object,
            new Mock<ICacheManager>().Object
        );

        var pricingResponse = apiService.OEPricingMultipleV4(
            new OEPricingMultipleV4Request { Request = new Request() }
        );

        var priceOut = pricingResponse.Response.PriceOutV2Collection.PriceOutV2s.Single();
        priceOut.Price.Should().Be(default);
        priceOut.Extamt.Should().Be(default);
    }

    [Test]
    public void OEPricingMultipleV4_Should_Always_Include_Authorization_Header_With_Bearer_Token()
    {
        var token = "bearerToken";
        var cacheManager = new Mock<ICacheManager>();
        cacheManager.Setup(o => o.Contains(SXeApiServiceV11.TokenCacheKey)).Returns(true);
        cacheManager.Setup(o => o.Get<string>(SXeApiServiceV11.TokenCacheKey)).Returns(token);
        var httpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>(
                (req, _) =>
                {
                    req.Headers.Should().Contain(o => o.Key == "Authorization");
                    req.Headers.Authorization.Parameter.Should().Be(token);
                }
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        @"
{
    ""response"": {
        ""cErrorMessage"": """",
        ""tOemultprcoutV2"": {
            ""t-oemultprcoutV2"": [
                {
                    ""prod"": ""CBD2 SIZES: 12X12 TO 24X24"",
                    ""price"": null,
                    ""extamt"": null
                }
            ]
        },
        ""tOemultprcoutbrk"": {
            ""t-oemultprcoutbrk"": []
        }
    }
}
"
                    )
                }
            );
        var httpClient = new HttpClient(httpMessageHandler.Object);
        var httpClientProvider = new Mock<HttpClientProvider>();
        httpClientProvider
            .Setup(o => o.GetHttpClient(It.IsAny<Uri>(), It.IsAny<HttpClientHandler>()))
            .Callback<Uri, HttpClientHandler>((uri, handler) => httpClient.BaseAddress = uri)
            .Returns(httpClient);
        var apiService = new SXeApiServiceV11(
            "http://localhost",
            "http://localhost",
            "operatorInitials",
            "operatorPassword",
            123,
            "tokenClientId",
            "tokenClientSecret",
            "tokenUserName",
            "tokenPassword",
            httpClientProvider.Object,
            cacheManager.Object
        );

        apiService.OEPricingMultipleV4(new OEPricingMultipleV4Request { Request = new Request() });
    }

    [Test]
    public void ARCustomerMnt_Should_Return_Data_From_Response_When_Bad_Request_Error_Returned()
    {
        var cacheManager = new Mock<ICacheManager>();
        cacheManager.Setup(o => o.Contains(SXeApiServiceV11.TokenCacheKey)).Returns(true);
        cacheManager
            .Setup(o => o.Get<string>(SXeApiServiceV11.TokenCacheKey))
            .Returns("bearerToken");
        var httpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent(
                        @"
{
""response"": {
                    ""cErrorMessage"": ""Set#: 1 Fld Name: slsrepout Msg: Salesrep Not Set Up in Salesrep Setup - OESS (4604)|Set#: 1 Fld Name: geocode Msg: GeoTAX AssignGeoTAXInfoService: Web Service Not Connected (6888)"",
                    ""returnData"": ""Set#: 1 Update Successful, Cono: 101 Customer #: 1194124""
                }
        }
"
                    )
                }
            );
        var httpClient = new HttpClient(httpMessageHandler.Object);
        var httpClientProvider = new Mock<HttpClientProvider>();
        httpClientProvider
            .Setup(o => o.GetHttpClient(It.IsAny<Uri>(), It.IsAny<HttpClientHandler>()))
            .Callback<Uri, HttpClientHandler>((uri, handler) => httpClient.BaseAddress = uri)
            .Returns(httpClient);
        var apiService = new SXeApiServiceV11(
            "http://localhost",
            "http://localhost",
            "operatorInitials",
            "operatorPassword",
            123,
            "tokenClientId",
            "tokenClientSecret",
            "tokenUserName",
            "tokenPassword",
            httpClientProvider.Object,
            cacheManager.Object
        );

        var pricingResponse = apiService.ARCustomerMnt(
            new ARCustomerMntRequest()
            {
                Request = new SXe.V11.IonApi.Models.ARCustomerMnt.Request()
            }
        );

        pricingResponse.Response.ReturnData
            .Should()
            .Be("Set#: 1 Update Successful, Cono: 101 Customer #: 1194124");
    }
}
