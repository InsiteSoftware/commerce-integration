namespace Insite.Integration.Connector.CloudSuiteDistribution.Tests.V11.Pipelines.GetAccountsReceivable;

using System;
using FluentAssertions;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.CloudSuiteDistribution.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.SfCustomerSummary;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetAccountsReceivable;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<GetAccountsReceivableParameter, GetAccountsReceivableResult>
{
    private Mock<ICloudSuiteDistributionClient> cloudSuiteDistributionClient;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.cloudSuiteDistributionClient = this.container.GetMock<ICloudSuiteDistributionClient>();

        this.dependencyLocator
            .Setup(o => o.GetInstance<ICloudSuiteDistributionClient>())
            .Returns(this.cloudSuiteDistributionClient.Object);
    }

    [Test]
    public void Order_Is_300()
    {
        this.pipe.Order.Should().Be(300);
    }

    [Test]
    public void Execute_Should_Call_GetAccountsReceivable_And_Set_Responses_To_Result()
    {
        var serializedSfCustomerSummaryResponse = "SerializedResponse";

        var parameter = this.GetDefaultParameter();

        var result = new GetAccountsReceivableResult
        {
            SerializedSfCustomerSummaryRequest = "SerializedRequest",
            SfCustomerSummaryRequest = new SfCustomerSummaryRequest()
            {
                Request = new Request()
                {
                    OperatorPassword = "Password_Value",
                    OperatorInit = "Username_Value",
                    CompanyNumber = 77777,
                }
            }
        };

        this.WhenGetAccountsReceivableIs(
            parameter.IntegrationConnection,
            result.SerializedSfCustomerSummaryRequest,
            ResultCode.Success,
            serializedSfCustomerSummaryResponse
        );

        result = this.RunExecute(parameter, result);

        this.VerifyGetAccountsReceivableWasCalled(
            parameter.IntegrationConnection,
            result.SerializedSfCustomerSummaryRequest
        );
        result.SerializedSfCustomerSummaryResponse.Should().Be(serializedSfCustomerSummaryResponse);
    }

    [Test]
    public void Execute_Should_Return_Error_When_GetAccountsReceivable_Returns_Error()
    {
        var serializedSfCustomerSummaryResponse = "SerializedResponse";

        var parameter = this.GetDefaultParameter();

        var result = new GetAccountsReceivableResult
        {
            SerializedSfCustomerSummaryRequest = "SerializedRequest",
            SfCustomerSummaryRequest = new SfCustomerSummaryRequest()
            {
                Request = new Request()
                {
                    OperatorPassword = "Password_Value",
                    OperatorInit = "Username_Value",
                    CompanyNumber = 77777,
                }
            }
        };

        this.WhenGetAccountsReceivableIs(
            parameter.IntegrationConnection,
            result.SerializedSfCustomerSummaryRequest,
            ResultCode.Error,
            serializedSfCustomerSummaryResponse
        );

        result = this.RunExecute(parameter, result);

        this.VerifyGetAccountsReceivableWasCalled(
            parameter.IntegrationConnection,
            result.SerializedSfCustomerSummaryRequest
        );
        result.ResultCode.Should().Be(ResultCode.Error);
        result.SubCode.Should().Be(SubCode.GeneralFailure);
    }

    protected override GetAccountsReceivableParameter GetDefaultParameter()
    {
        return new GetAccountsReceivableParameter
        {
            IntegrationConnection = new IntegrationConnection()
        };
    }

    private void WhenGetAccountsReceivableIs(
        IntegrationConnection integrationConnection,
        string request,
        ResultCode resultCode,
        string response
    )
    {
        this.cloudSuiteDistributionClient
            .Setup(o => o.GetAccountsReceivable(integrationConnection, request))
            .Returns((resultCode, response));
    }

    private void VerifyGetAccountsReceivableWasCalled(
        IntegrationConnection integrationConnection,
        string request
    )
    {
        this.cloudSuiteDistributionClient.Verify(
            o => o.GetAccountsReceivable(integrationConnection, request),
            Times.Once
        );
    }
}
