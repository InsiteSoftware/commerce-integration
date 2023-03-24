namespace Insite.WIS.Acumatica.Tests;

using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Insite.Common.HttpUtilities;
using Insite.Data.Entities;
using Insite.Integration.TestHelpers;
using Insite.WIS.Broker;
using Moq;
using NUnit.Framework;

[TestFixture]
public class IntegrationProcessorAcumaticaODataTests
{
    private Broker.WebIntegrationService.IntegrationJob integrationJob;
    private Mock<HttpClient> httpClient;
    private IntegrationProcessorAcumaticaOData processor;

    [SetUp]
    public void Setup()
    {
        this.integrationJob = new Broker.WebIntegrationService.IntegrationJob
        {
            JobDefinition = new Broker.WebIntegrationService.JobDefinition
            {
                IntegrationConnection = new Broker.WebIntegrationService.IntegrationConnection
                {
                    Url = "http://example.com"
                }
            }
        };

        this.httpClient = new Mock<HttpClient>();
        var httpClientProvider = new Mock<HttpClientProvider>();
        httpClientProvider
            .Setup(
                o =>
                    o.GetHttpClient(
                        It.Is<Uri>(
                            p =>
                                p.OriginalString
                                == this.integrationJob.JobDefinition.IntegrationConnection.Url
                        ),
                        null
                    )
            )
            .Returns<Uri, HttpClientHandler>(
                (uri, handler) =>
                {
                    this.httpClient.Object.BaseAddress = uri;
                    return this.httpClient.Object;
                }
            );

        this.processor = new IntegrationProcessorAcumaticaOData(httpClientProvider.Object);
    }

    [Test]
    public void Execute_Should_Create_Product_Data_When_Querying_For_Products()
    {
        var jobStep = new Broker.WebIntegrationService.JobDefinitionStep
        {
            SelectClause =
                "InventoryID,Description,TaxCategory,SalesUnit,ItemStatus,Weight,PriceClass,DefaultPrice,Type",
            FromClause = "ISC - Stock Items",
            Sequence = 1,
            ObjectName = nameof(Product)
        };

        this.httpClient
            .Setup(o => o.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(
                Task.FromResult(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(
                            @"
{
    ""value"":[
        {
            ""InventoryID"":""10002                         "",
            ""Description"":""3939 48Mm X 55M 9.0Mil Silver Cloth Duct Tape 24Rl/Cs"",
            ""TaxCategory"":""TAXABLE"",
            ""SalesUnit"":""RL"",
            ""ItemStatus"":""Active"",
            ""Weight"":""0.000000"",
            ""PriceClass"":null,
            ""DefaultPrice"":""9.980000"",
            ""Type"":""Finished Good""
        },
        {
            ""InventoryID"":""10003                         "",
            ""Description"":""3939 3\"" X 60Yd Silver Tartan Duct Tape 12Rl/Cs"",
            ""TaxCategory"":""TAXABLE"",
            ""SalesUnit"":""RL"",
            ""ItemStatus"":""Active"",
            ""Weight"":""0.000000"",
            ""PriceClass"":null,
            ""DefaultPrice"":""20.540000"",
            ""Type"":""Finished Good""
        }
    ]
}
        "
                        )
                    }
                )
            );

        var dataSet = this.RunExecute(jobStep);

        var productTable = dataSet.Tables["1Product"];
        productTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[]
                    {
                        "10002                         ",
                        "3939 48Mm X 55M 9.0Mil Silver Cloth Duct Tape 24Rl/Cs",
                        "TAXABLE",
                        "RL",
                        "Active",
                        "0.000000",
                        DBNull.Value,
                        "9.980000",
                        "Finished Good"
                    },
                    new object[]
                    {
                        "10003                         ",
                        @"3939 3"" X 60Yd Silver Tartan Duct Tape 12Rl/Cs",
                        "TAXABLE",
                        "RL",
                        "Active",
                        "0.000000",
                        DBNull.Value,
                        "20.540000",
                        "Finished Good"
                    }
                }
            );
    }

    [Test]
    public void Execute_Should_Throw_Exception_When_Erp_Response_Is_Null()
    {
        var jobStep = new Broker.WebIntegrationService.JobDefinitionStep
        {
            SelectClause =
                "CustomerID,CustomerName,AddressLine1,AddressLine2,City,State,PostalCode,Country,TaxZone,PriceClassID,Warehouse",
            FromClause = "ISC - Customer Refresh",
            Sequence = 1,
            ObjectName = nameof(Customer)
        };

        this.httpClient
            .Setup(o => o.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<HttpResponseMessage>(null));

        this.Invoking(o => o.RunExecute(jobStep))
            .Should()
            .Throw<Exception>()
            .WithMessage("Error calling API. Response is null.");
    }

    [Test]
    public void Execute_Should_Throw_Exception_When_Erp_Returns_Error_Response()
    {
        var jobStep = new Broker.WebIntegrationService.JobDefinitionStep
        {
            SelectClause =
                "CustomerID,CustomerName,AddressLine1,AddressLine2,City,State,PostalCode,Country,TaxZone,PriceClassID,Warehouse",
            FromClause = "ISC - Customer Refresh",
            Sequence = 1,
            ObjectName = nameof(Customer)
        };

        this.httpClient
            .Setup(o => o.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(
                Task.FromResult(
                    new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.InternalServerError,
                        ReasonPhrase = "Internal Server Error",
                        Content = new StringContent("foobar")
                    }
                )
            );

        this.Invoking(o => o.RunExecute(jobStep))
            .Should()
            .Throw<Exception>()
            .WithMessage(
                $@"Error calling API. Status Code: {HttpStatusCode.InternalServerError}. Reason Phrase: Internal Server Error. Content: foobar"
            );
    }

    private DataSet RunExecute(Broker.WebIntegrationService.JobDefinitionStep jobStep)
    {
        return this.processor.Execute(new SiteConnection(), this.integrationJob, jobStep);
    }
}
