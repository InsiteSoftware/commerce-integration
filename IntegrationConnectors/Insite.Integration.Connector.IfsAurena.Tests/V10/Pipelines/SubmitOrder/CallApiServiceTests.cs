namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitOrder;

using System;
using FluentAssertions;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests : BaseForPipeTests<SubmitOrderParameter, SubmitOrderResult>
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
    public void Order_Is_300()
    {
        this.pipe.Order.Should().Be(300);
    }

    [Test]
    public void Execute_Should_Call_SubmitOrder_And_Set_Response_To_Result()
    {
        var request = "Request";
        var response = "Response";

        var parameter = new SubmitOrderParameter
        {
            IntegrationConnection = new IntegrationConnection()
        };
        var result = new SubmitOrderResult { SerializedCustomerOrderRequest = request };

        this.WhenSubmitOrderIs(
            parameter.IntegrationConnection,
            request,
            ResultCode.Success,
            response
        );

        result = this.RunExecute(parameter, result);

        this.VerifyCustomerOrderWasCalled(parameter.IntegrationConnection, request);
        result.SerializedCustomerOrderResponse.Should().Be(response);
    }

    [Test]
    public void Execute_Should_Return_Error_When_SubmitOrder_Returns_Error()
    {
        var request = "Request";
        var response = "ErrorMessage";

        var parameter = new SubmitOrderParameter
        {
            IntegrationConnection = new IntegrationConnection()
        };
        var result = new SubmitOrderResult { SerializedCustomerOrderRequest = request };

        this.WhenSubmitOrderIs(
            parameter.IntegrationConnection,
            request,
            ResultCode.Error,
            response
        );

        result = this.RunExecute(parameter, result);

        this.VerifyCustomerOrderWasCalled(parameter.IntegrationConnection, request);
        result.ResultCode.Should().Be(ResultCode.Error);
        result.SubCode.Should().Be(SubCode.GeneralFailure);
        result.Message.Should().Be(response);
    }

    private void WhenSubmitOrderIs(
        IntegrationConnection integrationConnection,
        string request,
        ResultCode resultCode,
        string response
    )
    {
        this.ifsAurenaClient
            .Setup(o => o.SubmitOrder(integrationConnection, request))
            .Returns((resultCode, response));
    }

    private void VerifyCustomerOrderWasCalled(
        IntegrationConnection integrationConnection,
        string request
    )
    {
        this.ifsAurenaClient.Verify(o => o.SubmitOrder(integrationConnection, request), Times.Once);
    }
}
