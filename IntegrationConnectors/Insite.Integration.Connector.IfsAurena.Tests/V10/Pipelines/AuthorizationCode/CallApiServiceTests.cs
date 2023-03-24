namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.AuthorizationCode;

using System;
using FluentAssertions;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitAuthorizationCode;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<SubmitAuthorizationCodeParameter, SubmitAuthorizationCodeResult>
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
    public void Execute_Should_Call_AuthorizationCode_For_Each_SerializedAuthorizationCodeRequest_And_Set_Responses_To_Result()
    {
        var authorizationCodeRequest = "Request";

        var parameter = this.GetDefaultParameter();

        var result = new SubmitAuthorizationCodeResult
        {
            SerializedAuthorizationCodeRequest = authorizationCodeRequest
        };

        this.WhenSubmitAuthorizationCodeIs(
            parameter.IntegrationConnection,
            authorizationCodeRequest,
            ResultCode.Success,
            string.Empty
        );
        result = this.RunExecute(parameter, result);

        this.VerifySubmitAuthorizationCodeWasCalled(
            parameter.IntegrationConnection,
            authorizationCodeRequest
        );

        result.ResultCode.Should().Be(ResultCode.Success);
    }

    [Test]
    public void Execute_Should_Return_Error_When_AuthorizationCode_Returns_Error()
    {
        var authorizationCodeRequest = "Request";

        var parameter = this.GetDefaultParameter();

        var result = new SubmitAuthorizationCodeResult
        {
            SerializedAuthorizationCodeRequest = authorizationCodeRequest
        };

        this.WhenSubmitAuthorizationCodeIs(
            parameter.IntegrationConnection,
            authorizationCodeRequest,
            ResultCode.Error,
            string.Empty
        );

        result = this.RunExecute(parameter, result);

        this.VerifySubmitAuthorizationCodeWasCalled(
            parameter.IntegrationConnection,
            authorizationCodeRequest
        );
        result.ResultCode.Should().Be(ResultCode.Error);
        result.SubCode.Should().Be(SubCode.GeneralFailure);
    }

    protected override SubmitAuthorizationCodeParameter GetDefaultParameter()
    {
        return new SubmitAuthorizationCodeParameter
        {
            ErpOrderNumber = "Erp123",
            IntegrationConnection = new IntegrationConnection()
        };
    }

    private void WhenSubmitAuthorizationCodeIs(
        IntegrationConnection integrationConnection,
        string request,
        ResultCode resultCode,
        string response
    )
    {
        this.ifsAurenaClient
            .Setup(o => o.SubmitAuthorizationCode(integrationConnection, request))
            .Returns((resultCode, response));
    }

    private void VerifySubmitAuthorizationCodeWasCalled(
        IntegrationConnection integrationConnection,
        string request
    )
    {
        this.ifsAurenaClient.Verify(
            o => o.SubmitAuthorizationCode(integrationConnection, request),
            Times.Once
        );
    }
}
