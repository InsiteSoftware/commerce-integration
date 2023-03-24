namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.GetInventoryStock;

using System;
using FluentAssertions;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetInventoryStock;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<GetInventoryStockParameter, GetInventoryStockResult>
{
    private Mock<IIfsAurenaClient> ifsAurenaClient;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.ifsAurenaClient = this.container.GetMock<IIfsAurenaClient>();

        this.dependencyLocator
            .Setup(o => o.GetInstance<IIfsAurenaClient>())
            .Returns(this.ifsAurenaClient.Object);
    }

    [Test]
    public void Order_Is_200()
    {
        this.pipe.Order.Should().Be(200);
    }

    [Test]
    public void Execute_Should_Call_GetInventoryStock_And_Set_Result_To_SerializeInventoryPartInStockResponse()
    {
        var result = this.GetDefaultResult();
        var response = "TheResponse";

        this.WhenGetInventoryStockIs(
            null,
            result.InventoryPartInStockRequest,
            ResultCode.Success,
            response
        );

        result = this.RunExecute(result);

        this.VerifyGetInventoryStockWasCalled(null, result.InventoryPartInStockRequest);
        result.SerializedInventoryPartInStockResponse.Should().Be(response);
    }

    public void Execute_Should_Return_Error_When_GetInventoryStock_Returns_Error()
    {
        var result = this.GetDefaultResult();
        var response = "ErrorMessage";

        this.WhenGetInventoryStockIs(
            null,
            result.InventoryPartInStockRequest,
            ResultCode.Error,
            response
        );

        result = this.RunExecute(result);

        this.VerifyGetInventoryStockWasCalled(null, result.InventoryPartInStockRequest);
        result.ResultCode.Should().Be(ResultCode.Error);
        result.SubCode.Should().Be(SubCode.RealTimeInventoryGeneralFailure);
        result.Message.Should().Be(response);
    }

    private void WhenGetInventoryStockIs(
        IntegrationConnection integrationConnection,
        string request,
        ResultCode resultCode,
        string response
    )
    {
        this.ifsAurenaClient
            .Setup(o => o.GetInventoryStock(integrationConnection, request))
            .Returns((resultCode, response));
    }

    private void VerifyGetInventoryStockWasCalled(
        IntegrationConnection integrationConnection,
        string request
    )
    {
        this.ifsAurenaClient.Verify(
            o => o.GetInventoryStock(integrationConnection, request),
            Times.Once
        );
    }
}
