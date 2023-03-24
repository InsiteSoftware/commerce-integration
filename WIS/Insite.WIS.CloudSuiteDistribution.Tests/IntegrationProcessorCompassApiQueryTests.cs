namespace Insite.WIS.CloudSuiteDistribution.Tests;

using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Insite.Common.Helpers;
using Insite.Common.HttpUtilities;
using Insite.Integration.Enums;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Plugins;
using Insite.WIS.Broker.Providers;
using Insite.WIS.Broker.WebIntegrationService;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

[TestFixture]
public class IntegrationProcessorCompassApiQueryTests
{
    private const string Invalid = "Invalid";

    private Mock<HttpClientProvider> httpClientProvider;

    private Mock<OAuthTokenProvider> oauthTokenProvider;

    private Mock<IIntegrationJobLogger> integrationJobLogger;

    private Mock<HttpClient> httpClient;

    private static readonly JsonSerializerSettings JsonSerializerSettings =
        new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

    private IntegrationProcessorCompassApiQuery integrationProcessorCompassApiQuery;

    [SetUp]
    public void SetUp()
    {
        this.httpClientProvider = new Mock<HttpClientProvider>();
        this.oauthTokenProvider = new Mock<OAuthTokenProvider>();
        this.integrationJobLogger = new Mock<IIntegrationJobLogger>();
        this.httpClient = new Mock<HttpClient>();
        this.httpClientProvider
            .Setup(o => o.GetHttpClient(It.IsAny<Uri>(), null))
            .Returns(this.httpClient.Object);
        this.integrationProcessorCompassApiQuery = new IntegrationProcessorCompassApiQuery(
            this.httpClientProvider.Object,
            this.oauthTokenProvider.Object
        )
        {
            IntegrationJobLogger = this.integrationJobLogger.Object
        };
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(Invalid)]
    public void SubmitJob_Should_Return_Invalid_Json_When_Result_Is_Invalid_Json(string json)
    {
        this.WhenSendAsyncIs(json);

        var (queryId, errorMessage) = IntegrationProcessorCompassApiQuery.SubmitJob(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            new Dictionary<string, string>()
        );

        queryId.Should().BeNull();
        errorMessage
            .Should()
            .StartWith(
                string.Format(
                    IntegrationProcessorCompassApiQuery.InvalidSubmitJobResponseMessage,
                    "Error parsing JSON with exception"
                )
            );
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SubmitJob_Should_Return_Invalid_Status_When_Status_Is_Invalid(bool isNull)
    {
        this.WhenSendAsyncIs(isNull ? "{}" : $"{{ 'status': '{Invalid}' }}");

        var (queryId, errorMessage) = IntegrationProcessorCompassApiQuery.SubmitJob(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            new Dictionary<string, string>()
        );

        queryId.Should().BeNull();
        errorMessage
            .Should()
            .StartWith(
                string.Format(
                    IntegrationProcessorCompassApiQuery.InvalidSubmitJobResponseMessage,
                    $"Invalid status '{(isNull ? "null" : Invalid)}'"
                )
            );
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SubmitJob_Should_Return_Invalid_QueryId_When_QueryId_Is_Invalid(bool isNull)
    {
        var response = new CompassResponse(
            IntegrationProcessorCompassApiQuery.RunningStatus,
            isNull ? null : string.Empty
        );
        this.WhenSendAsyncIs(JsonConvert.SerializeObject(response, JsonSerializerSettings));

        var (queryId, errorMessage) = IntegrationProcessorCompassApiQuery.SubmitJob(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            new Dictionary<string, string>()
        );

        queryId.Should().BeNull();
        errorMessage
            .Should()
            .StartWith(
                string.Format(
                    IntegrationProcessorCompassApiQuery.InvalidSubmitJobResponseMessage,
                    $"Invalid query id"
                )
            );
    }

    [Test]
    public void SubmitJob_Should_Return_QueryId_When_Successful()
    {
        var response = new CompassResponse(
            IntegrationProcessorCompassApiQuery.RunningStatus,
            Guid.NewGuid().ToString()
        );
        this.WhenSendAsyncIs(JsonConvert.SerializeObject(response, JsonSerializerSettings));

        var (queryId, errorMessage) = IntegrationProcessorCompassApiQuery.SubmitJob(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            new Dictionary<string, string>()
        );

        queryId.Should().Be(response.QueryId);
        errorMessage.Should().BeNull();
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(Invalid)]
    public void PollForJobCompletion_Should_Return_Invalid_Json_When_Result_Is_Invalid_Json(
        string json
    )
    {
        this.WhenSendAsyncIs(json);

        var (rowCount, errorMessage) = IntegrationProcessorCompassApiQuery.PollForJobCompletion(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            new Dictionary<string, string>()
        );

        rowCount.Should().Be(-1);
        errorMessage
            .Should()
            .StartWith(
                string.Format(
                    IntegrationProcessorCompassApiQuery.InvalidJobStatusResponseMessage,
                    "Error parsing JSON with exception"
                )
            );
    }

    [TestCase(true)]
    [TestCase(false)]
    public void PollForJobCompletion_Should_Return_Invalid_Status_When_Status_Is_Invalid(
        bool isNull
    )
    {
        this.WhenSendAsyncIs(isNull ? "{}" : $"{{ 'status': '{Invalid}' }}");

        var (rowCount, errorMessage) = IntegrationProcessorCompassApiQuery.PollForJobCompletion(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            new Dictionary<string, string>(),
            false
        );

        rowCount.Should().Be(-1);
        errorMessage
            .Should()
            .StartWith(
                string.Format(
                    IntegrationProcessorCompassApiQuery.InvalidJobStatusResponseMessage,
                    $"Invalid status '{(isNull ? "null" : Invalid)}'"
                )
            );
    }

    [Test]
    public void PollForJobCompletion_Should_Return_Invalid_RowCount_When_RowCount_Is_Invalid()
    {
        var response = new CompassResponse(
            IntegrationProcessorCompassApiQuery.FinishedStatus,
            Guid.NewGuid().ToString()
        );
        this.WhenSendAsyncIs(JsonConvert.SerializeObject(response, JsonSerializerSettings));

        var (rowCount, errorMessage) = IntegrationProcessorCompassApiQuery.PollForJobCompletion(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            new Dictionary<string, string>()
        );

        rowCount.Should().Be(-1);
        errorMessage
            .Should()
            .StartWith(
                string.Format(
                    IntegrationProcessorCompassApiQuery.InvalidJobStatusResponseMessage,
                    $"Invalid row count"
                )
            );
    }

    [Test]
    public void PollForJobCompletion_Should_Return_Timeout_When_Timeout_Reached()
    {
        IntegrationProcessorCompassApiQuery.IntegrationJobTimeout = TimeSpan.Zero;
        var response = new CompassResponse(
            IntegrationProcessorCompassApiQuery.RunningStatus,
            Guid.NewGuid().ToString()
        );
        this.WhenSendAsyncIs(JsonConvert.SerializeObject(response, JsonSerializerSettings));

        var (rowCount, errorMessage) = IntegrationProcessorCompassApiQuery.PollForJobCompletion(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            new Dictionary<string, string>()
        );

        rowCount.Should().Be(-1);
        errorMessage
            .Should()
            .StartWith(
                string.Format(
                    IntegrationProcessorCompassApiQuery.InvalidJobStatusResponseMessage,
                    "Timeout of"
                )
            );
    }

    [Test]
    public void PollForJobCompletion_Should_Return_RowCount_When_Successful()
    {
        var response = new CompassResponse(
            IntegrationProcessorCompassApiQuery.FinishedStatus,
            Guid.NewGuid().ToString(),
            long.MaxValue
        );
        this.WhenSendAsyncIs(JsonConvert.SerializeObject(response, JsonSerializerSettings));

        var (rowCount, errorMessage) = IntegrationProcessorCompassApiQuery.PollForJobCompletion(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            new Dictionary<string, string>()
        );

        rowCount.Should().Be(long.MaxValue);
        errorMessage.Should().BeNull();
    }

    [Test]
    public void GetJobResults_Should_Return_Invalid_Response_When_Response_Is_Null()
    {
        this.WhenSendAsyncIs(null);

        var (dataTable, errorMessage) = IntegrationProcessorCompassApiQuery.GetJobResults(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            1,
            0,
            string.Empty,
            this.integrationJobLogger.Object,
            new Dictionary<string, string>()
        );

        dataTable.Rows.Count.Should().Be(0);
        errorMessage
            .Should()
            .StartWith(
                string.Format(
                    IntegrationProcessorCompassApiQuery.InvalidJobResultsResponseMessage,
                    "response is null"
                )
            );
    }

    [Test]
    public void GetJobResults_Should_Log_Warning_When_Response_Is_Empty()
    {
        this.WhenSendAsyncIs(string.Empty);

        var (dataTable, errorMessage) = IntegrationProcessorCompassApiQuery.GetJobResults(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            1,
            0,
            string.Empty,
            this.integrationJobLogger.Object,
            new Dictionary<string, string>()
        );

        this.VerifyWarningLogged("No rows returned");
        dataTable.Rows.Count.Should().Be(0);
        errorMessage.Should().BeNull();
    }

    [Test]
    public void GetJobResults_Should_Create_DataTable_Columns_With_First_Row()
    {
        this.WhenSendAsyncIs($"column1,column2{Environment.NewLine}value1,value");

        var (dataTable, errorMessage) = IntegrationProcessorCompassApiQuery.GetJobResults(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            1,
            0,
            string.Empty,
            this.integrationJobLogger.Object,
            new Dictionary<string, string>()
        );

        dataTable.Columns.Count.Should().Be(2);
        dataTable.Columns[0].ColumnName.Should().Be("column1");
        dataTable.Columns[1].ColumnName.Should().Be("column2");
        errorMessage.Should().BeNull();
    }

    [Test]
    public void GetJobResults_Should_Return_Timeout_When_Timeout_Reached()
    {
        IntegrationProcessorCompassApiQuery.IntegrationJobTimeout = TimeSpan.Zero;
        this.WhenSendAsyncIs(
            $"column1,column2{Environment.NewLine}value1,\"value,2\"{Environment.NewLine}value3,value4"
        );

        var (dataTable, errorMessage) = IntegrationProcessorCompassApiQuery.GetJobResults(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            1,
            0,
            string.Empty,
            this.integrationJobLogger.Object,
            new Dictionary<string, string>()
        );

        errorMessage
            .Should()
            .StartWith(
                string.Format(
                    IntegrationProcessorCompassApiQuery.InvalidJobResultsResponseMessage,
                    "Timeout of"
                )
            );
    }

    [Test]
    public void GetJobResults_Should_Populate_DataTable_Rows()
    {
        this.WhenSendAsyncIs(
            $"column1,column2{Environment.NewLine}value1,\"value,2\"{Environment.NewLine}value3,value4{Environment.NewLine}value5"
        );

        var (dataTable, errorMessage) = IntegrationProcessorCompassApiQuery.GetJobResults(
            this.httpClient.Object,
            string.Empty,
            string.Empty,
            1,
            0,
            string.Empty,
            this.integrationJobLogger.Object,
            new Dictionary<string, string>()
        );

        this.VerifyInfoLogged($"Processed rows {dataTable.Rows.Count} in");
        this.VerifyWarningLogged("Number of values 1 does not match number of columns 2");
        dataTable.Rows.Count.Should().Be(2);
        dataTable
            .AsEnumerable()
            .Select(o => o.Field<string>("column1"))
            .Should()
            .BeEquivalentTo(new[] { "value1", "value3" });
        dataTable
            .AsEnumerable()
            .Select(o => o.Field<string>("column2"))
            .Should()
            .BeEquivalentTo(new[] { "value,2", "value4" });
        errorMessage.Should().BeNull();
    }

    [Test]
    public void GetPageSize_Should_Return_Default_PageSize_When_No_Parameters()
    {
        var result = IntegrationProcessorCompassApiQuery.GetPageSize(
            new IntegrationJob(),
            new JobDefinitionStep()
        );

        result.Should().Be(IntegrationProcessorCompassApiQuery.DefaultPageSize);
    }

    [Test]
    public void GetPageSize_Should_Return_Job_Step_Parameter_PageSize_When_Available()
    {
        var jobParameterId = Guid.NewGuid();
        const int expected = 100;

        var result = IntegrationProcessorCompassApiQuery.GetPageSize(
            new IntegrationJob
            {
                IntegrationJobParameters = new[]
                {
                    new IntegrationJobParameter
                    {
                        JobDefinitionStepParameterId = jobParameterId,
                        Value = expected.ToString()
                    }
                }
            },
            new JobDefinitionStep
            {
                JobDefinitionStepParameters = new[]
                {
                    new JobDefinitionStepParameter
                    {
                        Id = jobParameterId,
                        Name = IntegrationProcessorCompassApiQuery.CompassApiPageSize,
                    }
                }
            }
        );

        result.Should().Be(expected);
    }

    [Test]
    public void GetPageSize_Should_Return_Job_Parameter_PageSize_When_Available()
    {
        var jobParameterId = Guid.NewGuid();
        const int expected = 100;

        var result = IntegrationProcessorCompassApiQuery.GetPageSize(
            new IntegrationJob
            {
                IntegrationJobParameters = new[]
                {
                    new IntegrationJobParameter
                    {
                        JobDefinitionStepParameterId = jobParameterId,
                        Value = expected.ToString()
                    }
                },
                JobDefinition = new JobDefinition
                {
                    JobDefinitionParameters = new[]
                    {
                        new JobDefinitionParameter
                        {
                            Id = jobParameterId,
                            Name = IntegrationProcessorCompassApiQuery.CompassApiPageSize,
                        }
                    }
                }
            },
            new JobDefinitionStep()
        );

        result.Should().Be(expected);
    }

    [Test]
    public void GetPageSize_Should_Return_Default_PageSize_When_Parameter_Is_Invalid()
    {
        var jobParameterId = Guid.NewGuid();

        var result = IntegrationProcessorCompassApiQuery.GetPageSize(
            new IntegrationJob
            {
                IntegrationJobParameters = new[]
                {
                    new IntegrationJobParameter
                    {
                        JobDefinitionStepParameterId = jobParameterId,
                        Value = "Invalid"
                    }
                },
                JobDefinition = new JobDefinition
                {
                    JobDefinitionParameters = new[]
                    {
                        new JobDefinitionParameter
                        {
                            Id = jobParameterId,
                            Name = IntegrationProcessorCompassApiQuery.CompassApiPageSize,
                        }
                    }
                }
            },
            new JobDefinitionStep()
        );

        result.Should().Be(IntegrationProcessorCompassApiQuery.DefaultPageSize);
    }

    [Test]
    [Ignore("Integration test for calling the API")]
    public void Integration_Test_Should_Get_Results()
    {
        var integrationProcessor = new IntegrationProcessorCompassApiQuery(
            new HttpClientProvider(),
            new OAuthTokenProvider()
        )
        {
            IntegrationJobLogger = this.integrationJobLogger.Object
        };

#pragma warning disable CS0618 // Type or member is obsolete
        var result = integrationProcessor.Execute(
            new SiteConnection(),
            new IntegrationJob { JobDefinition = new JobDefinition { RefreshBatchSize = 0 } },
            new JobDefinitionStep
            {
                ObjectName = "Customer",
                IntegrationQuery = "select cast(custno as int) as custno, name, city from arsc",
                IntegrationConnectionOverride = new IntegrationConnection
                {
                    TypeName = nameof(IntegrationConnectionType.ApiRopcEndpoint),
                    DataSource =
                        "https://mingle-sso.inforcloudsuite.com:443/BANGERSUSA_TST/as/token.oauth2", // token endpoint
                    LogOn = "Get Value from BA", // client id
                    Password = EncryptionHelper.EncryptAes("Get Value from BA"), // client secret
                    GatewayHost = "Get Value from BA", // username
                    GatewayService = EncryptionHelper.EncryptAes("Get Value from BA"), // password
                    Url = "https://mingle-ionapi.inforcloudsuite.com", // api endpoint
                    Client = "/BANGERSUSA_TST/DATAFABRIC/compass/v2/jobs" // resource
                }
            }
        );
#pragma warning restore

        result.Should().NotBeNull();
    }

    private void WhenSendAsyncIs(string json = "")
    {
        this.httpClient
            .Setup(o => o.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .Returns(
                Task.FromResult(
                    new HttpResponseMessage
                    {
                        Content = json == null ? null : new StringContent(json)
                    }
                )
            );
    }

    private void VerifyInfoLogged(string message)
    {
        this.integrationJobLogger.Verify(
            o => o.Info(It.Is<string>(p => p.StartsWith(message)), true),
            Times.Once
        );
    }

    private void VerifyWarningLogged(string message)
    {
        this.integrationJobLogger.Verify(o => o.Warn(message, true), Times.Once);
    }

    private class CompassResponse
    {
        public CompassResponse(string status, string queryId, long? rowCount = null)
        {
            this.Status = status;
            this.QueryId = queryId;
            this.RowCount = rowCount;
        }

        public string Status { get; }

        public string QueryId { get; }

        public long? RowCount { get; }
    }
}
