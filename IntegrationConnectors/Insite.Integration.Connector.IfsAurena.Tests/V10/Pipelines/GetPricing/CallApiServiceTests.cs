namespace Insite.Integration.Connector.IfsAurena.Tests.V10.Pipelines.GetPricing;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.IfsAurena.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetPricing;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests : BaseForPipeTests<GetPricingParameter, GetPricingResult>
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
    public void Execute_Should_Call_GetPricing_For_Each_PriceQuery_Request_And_Set_Responses_To_Result()
    {
        var priceQueryRequest1 = "PriceQueryRequest1";
        var priceQueryRequest2 = "PriceQueryRequest2";
        var priceQueryResponse1 = "PriceQueryResponse1";
        var priceQueryResponse2 = "PriceQueryResponse2";

        var parameter = this.GetDefaultParameter();

        var result = new GetPricingResult
        {
            SerializedPriceQueryRequests = new List<string>
            {
                priceQueryRequest1,
                priceQueryRequest2
            }
        };

        this.WhenGetPricingIs(
            parameter.IntegrationConnection,
            priceQueryRequest1,
            ResultCode.Success,
            priceQueryResponse1
        );
        this.WhenGetPricingIs(
            parameter.IntegrationConnection,
            priceQueryRequest2,
            ResultCode.Success,
            priceQueryResponse2
        );

        result = this.RunExecute(parameter, result);

        this.VerifyGetPricingWasCalled(parameter.IntegrationConnection, priceQueryRequest1);
        this.VerifyGetPricingWasCalled(parameter.IntegrationConnection, priceQueryRequest2);
        result.SerializedPriceQueryResponses.Should().Contain(priceQueryResponse1);
        result.SerializedPriceQueryResponses.Should().Contain(priceQueryResponse2);
    }

    [Test]
    public void Execute_Should_Return_Error_When_GetPricing_Returns_Error()
    {
        var priceQueryRequest = "PriceQueryRequest";
        var priceQueryResponse = "ErrorMessage";

        var parameter = this.GetDefaultParameter();

        var result = new GetPricingResult
        {
            SerializedPriceQueryRequests = new List<string> { priceQueryRequest }
        };

        this.WhenGetPricingIs(
            parameter.IntegrationConnection,
            priceQueryRequest,
            ResultCode.Error,
            priceQueryResponse
        );

        result = this.RunExecute(parameter, result);

        this.VerifyGetPricingWasCalled(parameter.IntegrationConnection, priceQueryRequest);
        result.ResultCode.Should().Be(ResultCode.Error);
        result.SubCode.Should().Be(SubCode.RealTimePricingGeneralFailure);
    }

    [Test]
    public void Execute_Should_Call_CleanPriceQuery()
    {
        var parameter = this.GetDefaultParameter();

        this.RunExecute(parameter);

        this.ifsAurenaClient.Verify(
            o => o.CleanPriceQuery(parameter.IntegrationConnection),
            Times.Once
        );
    }

    protected override GetPricingParameter GetDefaultParameter()
    {
        return new GetPricingParameter { IntegrationConnection = new IntegrationConnection() };
    }

    private void WhenGetPricingIs(
        IntegrationConnection integrationConnection,
        string request,
        ResultCode resultCode,
        string response
    )
    {
        this.ifsAurenaClient
            .Setup(o => o.GetPricing(integrationConnection, request))
            .Returns((resultCode, response));
    }

    private void VerifyGetPricingWasCalled(
        IntegrationConnection integrationConnection,
        string request
    )
    {
        this.ifsAurenaClient.Verify(o => o.GetPricing(integrationConnection, request), Times.Once);
    }
}
