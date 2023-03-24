namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitOrderLines;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrderLines;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<SubmitOrderLinesParameter, SubmitOrderLinesResult>
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
    public void Execute_Should_Call_SubmitOrderLine_For_Each_SerializedCustomerOrderLineRequest_And_Set_Responses_To_Result()
    {
        var customerOrderLineRequest1 = "Request1";
        var customerOrderLineRequest2 = "Request2";
        var customerOrderLineResponse1 = "Response1";
        var customerOrderLineResponse2 = "Response2";

        var parameter = this.GetDefaultParameter();

        var result = new SubmitOrderLinesResult
        {
            SerializedCustomerOrderLineRequests = new List<string>
            {
                customerOrderLineRequest1,
                customerOrderLineRequest2
            }
        };

        this.WhenSubmitOrderLineIs(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            customerOrderLineRequest1,
            ResultCode.Success,
            customerOrderLineResponse1
        );
        this.WhenSubmitOrderLineIs(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            customerOrderLineRequest2,
            ResultCode.Success,
            customerOrderLineResponse2
        );

        result = this.RunExecute(parameter, result);

        this.VerifyCustomerOrderLineWasCalled(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            customerOrderLineRequest1
        );
        this.VerifyCustomerOrderLineWasCalled(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            customerOrderLineRequest2
        );
        result.SerializedCustomerOrderLineResponses.Should().Contain(customerOrderLineResponse1);
        result.SerializedCustomerOrderLineResponses.Should().Contain(customerOrderLineResponse2);
    }

    [Test]
    public void Execute_Should_Return_Error_When_SubmitOrderLine_Throws_Exception()
    {
        var customerOrderLineRequest = "Request";
        var customerOrderLineResponse = "ErrorMessage";

        var parameter = this.GetDefaultParameter();

        var result = new SubmitOrderLinesResult
        {
            SerializedCustomerOrderLineRequests = new List<string> { customerOrderLineRequest }
        };

        this.WhenSubmitOrderLineIs(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            customerOrderLineRequest,
            ResultCode.Error,
            customerOrderLineResponse
        );

        result = this.RunExecute(parameter, result);

        this.VerifyCustomerOrderLineWasCalled(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            customerOrderLineRequest
        );
        result.ResultCode.Should().Be(ResultCode.Error);
        result.SubCode.Should().Be(SubCode.GeneralFailure);
    }

    protected override SubmitOrderLinesParameter GetDefaultParameter()
    {
        return new SubmitOrderLinesParameter
        {
            ErpOrderNumber = "Erp123",
            IntegrationConnection = new IntegrationConnection()
        };
    }

    private void WhenSubmitOrderLineIs(
        IntegrationConnection integrationConnection,
        string erpOrderNumber,
        string request,
        ResultCode resultCode,
        string response
    )
    {
        this.ifsAurenaClient
            .Setup(o => o.SubmitOrderLine(integrationConnection, erpOrderNumber, request))
            .Returns((resultCode, response));
    }

    private void VerifyCustomerOrderLineWasCalled(
        IntegrationConnection integrationConnection,
        string erpOrderNumber,
        string request
    )
    {
        this.ifsAurenaClient.Verify(
            o => o.SubmitOrderLine(integrationConnection, erpOrderNumber, request),
            Times.Once
        );
    }
}
