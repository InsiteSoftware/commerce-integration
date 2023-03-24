namespace Insite.Integration.Connector.CloudSuiteDistribution.Tests.V11.Pipelines.GetPricingAndInventoryStock;

using System;
using FluentAssertions;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.CloudSuiteDistribution.Services;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.OePricingMultipleV4;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetPricingAndInventoryStock;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
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
    public void Execute_Should_Call_GetPricingAndInventoryStock_And_Set_Responses_To_Result()
    {
        var serializedOePricingMultipleV4Response = "SerializedResponse";

        var parameter = this.GetDefaultParameter();

        var result = new GetPricingAndInventoryStockResult
        {
            SerializedOePricingMultipleV4Request = "SerializedRequest",
            OePricingMultipleV4Request = new OePricingMultipleV4Request()
            {
                Request = new Request()
                {
                    OperatorPassword = "Password_Value",
                    OperatorInit = "Username_Value",
                    CompanyNumber = 77777,
                }
            }
        };

        this.WhenGetPricingAndInventoryStockIs(
            parameter.IntegrationConnection,
            result.SerializedOePricingMultipleV4Request,
            ResultCode.Success,
            serializedOePricingMultipleV4Response
        );

        result = this.RunExecute(parameter, result);

        this.VerifyGetPricingAndInventoryStockWasCalled(
            parameter.IntegrationConnection,
            result.SerializedOePricingMultipleV4Request
        );
        result.SerializedOePricingMultipleV4Response
            .Should()
            .Be(serializedOePricingMultipleV4Response);
    }

    [Test]
    public void Execute_Should_Return_Error_When_GetPricingAndInventoryStock_Returns_Error()
    {
        var serializedOePricingMultipleV4Response = "SerializedResponse";

        var parameter = this.GetDefaultParameter();

        var result = new GetPricingAndInventoryStockResult
        {
            SerializedOePricingMultipleV4Request = "SerializedRequest",
            OePricingMultipleV4Request = new OePricingMultipleV4Request()
            {
                Request = new Request()
                {
                    OperatorPassword = "Password_Value",
                    OperatorInit = "Username_Value",
                    CompanyNumber = 77777,
                }
            }
        };

        this.WhenGetPricingAndInventoryStockIs(
            parameter.IntegrationConnection,
            result.SerializedOePricingMultipleV4Request,
            ResultCode.Error,
            serializedOePricingMultipleV4Response
        );

        result = this.RunExecute(parameter, result);

        this.VerifyGetPricingAndInventoryStockWasCalled(
            parameter.IntegrationConnection,
            result.SerializedOePricingMultipleV4Request
        );
        result.ResultCode.Should().Be(ResultCode.Error);
        result.SubCode.Should().Be(SubCode.GeneralFailure);
    }

    protected override GetPricingAndInventoryStockParameter GetDefaultParameter()
    {
        return new GetPricingAndInventoryStockParameter
        {
            IntegrationConnection = new IntegrationConnection()
        };
    }

    private void WhenGetPricingAndInventoryStockIs(
        IntegrationConnection integrationConnection,
        string request,
        ResultCode resultCode,
        string response
    )
    {
        this.cloudSuiteDistributionClient
            .Setup(o => o.GetPricingAndInventoryStock(integrationConnection, request))
            .Returns((resultCode, response));
    }

    private void VerifyGetPricingAndInventoryStockWasCalled(
        IntegrationConnection integrationConnection,
        string request
    )
    {
        this.cloudSuiteDistributionClient.Verify(
            o => o.GetPricingAndInventoryStock(integrationConnection, request),
            Times.Once
        );
    }
}
