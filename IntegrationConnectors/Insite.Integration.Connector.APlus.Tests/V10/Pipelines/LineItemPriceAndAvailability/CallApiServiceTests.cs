namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.LineItemPriceAndAvailability;

using System;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;
using Insite.Integration.Connector.APlus.Services;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    private Mock<IAPlusApiService> aPlusService;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.aPlusService = this.container.GetMock<IAPlusApiService>();

        var aPlusApiServiceFactory = this.container.GetMock<IAPlusApiServiceFactory>();
        aPlusApiServiceFactory
            .Setup(o => o.GetAPlusApiService(null))
            .Returns(this.aPlusService.Object);

        var dependencyLocator = this.container.GetMock<IDependencyLocator>();
        dependencyLocator
            .Setup(o => o.GetInstance<IAPlusApiServiceFactory>())
            .Returns(aPlusApiServiceFactory.Object);
    }

    [Test]
    public void Execute_Should_Get_Response()
    {
        var lineItemPriceAndAvailabilityRequest = new LineItemPriceAndAvailabilityRequest();
        var lineItemPriceAndAvailabilityResponse = new LineItemPriceAndAvailabilityResponse();

        this.WhenLineItemPriceAndAvailabilityIs(
            lineItemPriceAndAvailabilityRequest,
            lineItemPriceAndAvailabilityResponse
        );

        var result = new LineItemPriceAndAvailabilityResult()
        {
            LineItemPriceAndAvailabilityRequest = lineItemPriceAndAvailabilityRequest
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(
            lineItemPriceAndAvailabilityResponse,
            result.LineItemPriceAndAvailabilityResponse
        );
    }

    [Test]
    public void Execute_Should_Return_Error_When_LineItemPriceAndAvailability_Throws_Exception()
    {
        var lineItemPriceAndAvailabilityRequest = new LineItemPriceAndAvailabilityRequest();
        var exceptionMessage = "error message";

        this.WhenLineItemPriceAndAvailabilityThrowsException(
            lineItemPriceAndAvailabilityRequest,
            exceptionMessage
        );

        var result = new LineItemPriceAndAvailabilityResult()
        {
            LineItemPriceAndAvailabilityRequest = lineItemPriceAndAvailabilityRequest
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, exceptionMessage);
    }

    [Test]
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    protected void WhenLineItemPriceAndAvailabilityIs(
        LineItemPriceAndAvailabilityRequest lineItemPriceAndAvailabilityRequest,
        LineItemPriceAndAvailabilityResponse lineItemPriceAndAvailability
    )
    {
        this.aPlusService
            .Setup(o => o.LineItemPriceAndAvailability(lineItemPriceAndAvailabilityRequest))
            .Returns(lineItemPriceAndAvailability);
    }

    protected void WhenLineItemPriceAndAvailabilityThrowsException(
        LineItemPriceAndAvailabilityRequest lineItemPriceAndAvailabilityRequest,
        string exceptionMessage
    )
    {
        this.aPlusService
            .Setup(o => o.LineItemPriceAndAvailability(lineItemPriceAndAvailabilityRequest))
            .Throws(new Exception(exceptionMessage));
    }
}
