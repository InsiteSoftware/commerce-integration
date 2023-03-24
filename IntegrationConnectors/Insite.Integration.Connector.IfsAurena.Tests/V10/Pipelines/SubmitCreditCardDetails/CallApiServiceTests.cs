namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.SubmitCreditCardDetails;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitCreditCardDetails;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<SubmitCreditCardDetailsParameter, SubmitCreditCardDetailsResult>
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
    public void Execute_Should_Call_CreditCardDetails_For_Each_SerializedCreditCardDetailsRequest_And_Set_Responses_To_Result()
    {
        var creditCardDetailsRequest = "Request";

        var parameter = this.GetDefaultParameter();

        var result = new SubmitCreditCardDetailsResult
        {
            SerializedCreditCardDetailsRequest = creditCardDetailsRequest
        };

        this.WhenSubmitCreditCardDetailsIs(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            creditCardDetailsRequest,
            ResultCode.Success,
            string.Empty
        );
        result = this.RunExecute(parameter, result);

        this.VerifySubmitCreditCardDetailsWasCalled(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            creditCardDetailsRequest
        );

        result.ResultCode.Should().Be(ResultCode.Success);
    }

    [Test]
    public void Execute_Should_Return_Error_When_CreditCardDetails_Returns_Error()
    {
        var creditCardDetailsRequest = "Request";

        var parameter = this.GetDefaultParameter();

        var result = new SubmitCreditCardDetailsResult
        {
            SerializedCreditCardDetailsRequest = creditCardDetailsRequest
        };

        this.WhenSubmitCreditCardDetailsIs(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            creditCardDetailsRequest,
            ResultCode.Error,
            string.Empty
        );

        result = this.RunExecute(parameter, result);

        this.VerifySubmitCreditCardDetailsWasCalled(
            parameter.IntegrationConnection,
            parameter.ErpOrderNumber,
            creditCardDetailsRequest
        );
        result.ResultCode.Should().Be(ResultCode.Error);
        result.SubCode.Should().Be(SubCode.GeneralFailure);
    }

    protected override SubmitCreditCardDetailsParameter GetDefaultParameter()
    {
        return new SubmitCreditCardDetailsParameter
        {
            ErpOrderNumber = "Erp123",
            IntegrationConnection = new IntegrationConnection()
        };
    }

    private void WhenSubmitCreditCardDetailsIs(
        IntegrationConnection integrationConnection,
        string erpOrderNumber,
        string request,
        ResultCode resultCode,
        string response
    )
    {
        this.ifsAurenaClient
            .Setup(o => o.SubmitCreditCardDetails(integrationConnection, erpOrderNumber, request))
            .Returns((resultCode, response));
    }

    private void VerifySubmitCreditCardDetailsWasCalled(
        IntegrationConnection integrationConnection,
        string erpOrderNumber,
        string request
    )
    {
        this.ifsAurenaClient.Verify(
            o => o.SubmitCreditCardDetails(integrationConnection, erpOrderNumber, request),
            Times.Once
        );
    }
}
