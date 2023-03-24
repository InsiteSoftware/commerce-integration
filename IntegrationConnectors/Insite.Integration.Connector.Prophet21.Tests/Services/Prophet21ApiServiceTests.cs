namespace Insite.Integration.Connector.Prophet21.Tests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Insite.Common.Dependencies;
using Insite.Common.Helpers;
using Insite.Common.HttpUtilities;
using Insite.Core.Interfaces.Plugins.Caching;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.TestHelpers;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.Services;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.Entities;
using Moq;
using Moq.Protected;
using NUnit.Framework;

[TestFixture]
public class Prophet21ApiServiceTests
{
    private Mock<IProphet21IntegrationConnectionProvider> prophet21IntegrationConnectionProvider;

    private Mock<HttpClientProvider> httpClientProvider;

    private Mock<HttpMessageHandler> httpMessageHandler;

    private Mock<ICacheManager> cacheManager;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();
        this.httpClientProvider = new Mock<HttpClientProvider>();
        this.cacheManager = container.GetMock<ICacheManager>();
        this.cacheManager
            .Setup(
                o => o.GetOrAdd(It.IsAny<string>(), It.IsAny<Func<string>>(), It.IsAny<TimeSpan>())
            )
            .Returns<string, Func<string>, TimeSpan>((o, p, x) => p());
        this.prophet21IntegrationConnectionProvider =
            container.GetMock<IProphet21IntegrationConnectionProvider>();

        var dependencyLocator = container.GetMock<IDependencyLocator>();
        dependencyLocator
            .Setup(o => o.GetInstance<IProphet21IntegrationConnectionProvider>())
            .Returns(this.prophet21IntegrationConnectionProvider.Object);

        this.httpMessageHandler = new Mock<HttpMessageHandler>();

        this.httpClientProvider
            .Setup(o => o.GetHttpClient(It.IsAny<Uri>(), null))
            .Returns<Uri, HttpClientHandler>(
                (o, p) => new HttpClient(this.httpMessageHandler.Object) { BaseAddress = o }
            );

        TestHelper.MockSiteContext(null, dependencyLocator);
    }

    [Test]
    public void GetContacts_Should_Call_ConnectionProvider()
    {
        var array = new ArrayOfContact
        {
            Contacts = new List<Contact> { new Contact { Id = "guid" } }
        };
        this.httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(o => o.RequestUri.AbsolutePath.Contains("contact")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Content = new StringContent(Prophet21SerializationService.Serialize(array))
                }
            );

        var tokenResponse = new Token { AccessToken = "ABC" };
        this.httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(o => o.RequestUri.AbsolutePath.Contains("token")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Content = new StringContent(
                        Prophet21SerializationService.Serialize(tokenResponse)
                    )
                }
            );

        this.prophet21IntegrationConnectionProvider
            .Setup(o => o.GetIntegrationConnection(It.IsAny<IntegrationConnection>()))
            .Returns(new IntegrationConnection { Url = "https://trunk.local.com" });
        var service = new Prophet21ApiService(
            this.httpClientProvider.Object,
            this.cacheManager.Object
        );
        var contacts = service.GetContacts(null, "address");

        this.prophet21IntegrationConnectionProvider.Verify(
            o => o.GetIntegrationConnection(It.IsAny<IntegrationConnection>())
        );
        contacts.Contacts.First().Id.Should().Be("guid");
    }

    [TestCase("&%=?", "%26%25%3d%3f")]
    [TestCase("1234", "1234")]
    [TestCase("asdf", "asdf")]
    [Ignore("Request content is now encoded and unable to be checked.")]
    public void Special_Characters_In_Credentials_Should_Be_Encoded(
        string password,
        string expectedPassword
    )
    {
        var array = new ArrayOfContact
        {
            Contacts = new List<Contact> { new Contact { Id = "guid" } }
        };
        this.httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Content = new StringContent(Prophet21SerializationService.Serialize(array))
                }
            );

#pragma warning disable CS0618 // Type or member is obsolete
        var integrationConnection = new IntegrationConnection
        {
            Url = "https://trunk.local.com",
            Password = EncryptionHelper.EncryptAes(password)
        };
#pragma warning restore
        this.prophet21IntegrationConnectionProvider
            .Setup(o => o.GetIntegrationConnection(It.IsAny<IntegrationConnection>()))
            .Returns(integrationConnection);
        var service = new Prophet21ApiService(
            this.httpClientProvider.Object,
            this.cacheManager.Object
        );
        var contacts = service.GetContacts(null, "address");

        this.httpMessageHandler
            .Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(
                    p => this.CheckUri(p.RequestUri, $"password={expectedPassword}")
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        contacts.Contacts.First().Id.Should().Be("guid");
    }

    [Test]
    public void Verify_Get_Token()
    {
        var password = "testPassword";
        var tokenResponse = new Token { AccessToken = "ABC" };
        this.httpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Accepted,
                    Content = new StringContent(
                        Prophet21SerializationService.Serialize(tokenResponse)
                    )
                }
            );
#pragma warning disable CS0618 // Type or member is obsolete
        var integrationConnection = new IntegrationConnection
        {
            Url = "https://trunk.local.com",
            Password = EncryptionHelper.EncryptAes(password)
        };
#pragma warning restore
        this.prophet21IntegrationConnectionProvider
            .Setup(o => o.GetIntegrationConnection(It.IsAny<IntegrationConnection>()))
            .Returns(integrationConnection);
        var service = new Prophet21ApiService(
            this.httpClientProvider.Object,
            this.cacheManager.Object
        );
        var token = service.GetToken(integrationConnection);

        this.httpMessageHandler
            .Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(p => p.Content.IsFormData()),
                ItExpr.IsAny<CancellationToken>()
            );

        token.Should().Be("ABC");
    }

    private bool CheckUri(Uri requestUri, string query)
    {
        return requestUri.AbsolutePath.ContainsCaseInsensitive("api/security/token")
            && requestUri.Query.ContainsCaseInsensitive(query);
    }
}
