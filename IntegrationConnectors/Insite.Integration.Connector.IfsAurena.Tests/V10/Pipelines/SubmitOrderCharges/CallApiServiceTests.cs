namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitOrderCharges;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrderCharges;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<SubmitOrderChargesParameter, SubmitOrderChargesResult>
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
    public void Execute_Should_Call_SubmitOrderCharge_For_Each_SerializedCustomerOrderChargeRequest_And_Set_Responses_To_Result()
    {
        var customerOrderChargeRequest1 = "Request1";
        var customerOrderChargeRequest2 = "Request2";
        var customerOrderChargeResponse1 = "Response1";
        var customerOrderChargeResponse2 = "Response2";

        var parameter = this.GetDefaultParameter();

        var result = new SubmitOrderChargesResult
        {
            SerializedCustomerOrderChargeRequests = new List<string>
            {
                customerOrderChargeRequest1,
                customerOrderChargeRequest2
            }
        };

        this.WhenSubmitOrderChargeIs(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            customerOrderChargeRequest1,
            ResultCode.Success,
            customerOrderChargeResponse1
        );
        this.WhenSubmitOrderChargeIs(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            customerOrderChargeRequest2,
            ResultCode.Success,
            customerOrderChargeResponse2
        );

        result = this.RunExecute(parameter, result);

        this.VerifyCustomerOrderChargeWasCalled(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            customerOrderChargeRequest1
        );
        this.VerifyCustomerOrderChargeWasCalled(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            customerOrderChargeRequest2
        );
        result.SerializedCustomerOrderChargeResponses
            .Should()
            .Contain(customerOrderChargeResponse1);
        result.SerializedCustomerOrderChargeResponses
            .Should()
            .Contain(customerOrderChargeResponse2);
    }

    [Test]
    public void Execute_Should_Return_Error_When_SubmitOrderCharge_Returns_Error()
    {
        var customerOrderChargeRequest = "Request";
        var customerOrderChargeResponse = "ErrorMessage";

        var parameter = this.GetDefaultParameter();

        var result = new SubmitOrderChargesResult
        {
            SerializedCustomerOrderChargeRequests = new List<string> { customerOrderChargeRequest }
        };

        this.WhenSubmitOrderChargeIs(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            customerOrderChargeRequest,
            ResultCode.Error,
            customerOrderChargeResponse
        );

        result = this.RunExecute(parameter, result);

        this.VerifyCustomerOrderChargeWasCalled(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            customerOrderChargeRequest
        );
        result.ResultCode.Should().Be(ResultCode.Error);
        result.SubCode.Should().Be(SubCode.GeneralFailure);
    }

    protected override SubmitOrderChargesParameter GetDefaultParameter()
    {
        return new SubmitOrderChargesParameter
        {
            ErpOrderNumber = "Erp123",
            IntegrationConnection = new IntegrationConnection()
        };
    }

    private void WhenSubmitOrderChargeIs(
        IntegrationConnection integrationConnection,
        string erpOrderNumber,
        string request,
        ResultCode resultCode,
        string response
    )
    {
        this.ifsAurenaClient
            .Setup(o => o.SubmitOrderCharge(integrationConnection, erpOrderNumber, request))
            .Returns((resultCode, response));
    }

    private void VerifyCustomerOrderChargeWasCalled(
        IntegrationConnection integrationConnection,
        string erpOrderNumber,
        string request
    )
    {
        this.ifsAurenaClient.Verify(
            o => o.SubmitOrderCharge(integrationConnection, erpOrderNumber, request),
            Times.Once
        );
    }
}
