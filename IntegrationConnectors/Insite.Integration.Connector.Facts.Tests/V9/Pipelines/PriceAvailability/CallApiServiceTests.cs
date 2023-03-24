namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.PriceAvailability;

using System;
using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.Services;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;
using NUnit.Framework;
using Moq;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    private Mock<IFactsApiService> factsApiService;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.factsApiService = this.container.GetMock<IFactsApiService>();

        var factsApiServiceFactory = this.container.GetMock<IFactsApiServiceFactory>();
        factsApiServiceFactory
            .Setup(o => o.GetFactsApiService(null))
            .Returns(this.factsApiService.Object);

        var dependencyLocator = this.container.GetMock<IDependencyLocator>();
        dependencyLocator
            .Setup(o => o.GetInstance<IFactsApiServiceFactory>())
            .Returns(factsApiServiceFactory.Object);
    }

    [Test]
    public void Order_Is_700()
    {
        Assert.AreEqual(700, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_GetItemPrice_Reply()
    {
        var priceAvailabilityRequest = this.CreatePriceAvailabilityRequest(1);
        var priceAvailabilityResponse = new PriceAvailabilityResponse();

        this.WhenPriceAvailabilityIs(priceAvailabilityResponse);

        var result = new PriceAvailabilityResult
        {
            PriceAvailabilityRequest = priceAvailabilityRequest
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(priceAvailabilityResponse, result.PriceAvailabilityResponse);
    }

    [TestCase(1, 1)]
    [TestCase(19, 1)]
    [TestCase(20, 1)]
    [TestCase(21, 2)]
    [TestCase(39, 2)]
    [TestCase(40, 2)]
    [TestCase(41, 3)]
    public void Execute_Should_Page_RequestItems(int numberOfRequestItems, int numberOfPages)
    {
        var priceAvailabilityRequest = this.CreatePriceAvailabilityRequest(numberOfRequestItems);
        var priceAvailabilityResponse = new PriceAvailabilityResponse { Response = new Response() };

        this.WhenPriceAvailabilityIs(priceAvailabilityResponse);

        var result = new PriceAvailabilityResult
        {
            PriceAvailabilityRequest = priceAvailabilityRequest
        };
        this.RunExecute(result);

        this.VerifyPriceAvailabilityWasCalled(Times.Exactly(numberOfPages));
    }

    [Test]
    public void Execute_Should_Return_Error_When_PriceAvailability_Throws_Exception()
    {
        var priceAvailabilityRequest = this.CreatePriceAvailabilityRequest(1);
        var exception = new Exception("Test Exception Message");

        this.WhenPriceAvailabilityThrowsException(exception);

        var result = new PriceAvailabilityResult
        {
            PriceAvailabilityRequest = priceAvailabilityRequest
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(exception.Message, result.Message);
    }

    private PriceAvailabilityRequest CreatePriceAvailabilityRequest(int numberOfRequestItems)
    {
        var priceAvailabilityRequest = new PriceAvailabilityRequest { Request = new Request() };

        for (var i = 0; i < numberOfRequestItems; i++)
        {
            priceAvailabilityRequest.Request.Items.Add(new RequestItem());
        }

        return priceAvailabilityRequest;
    }

    private void WhenPriceAvailabilityIs(PriceAvailabilityResponse priceAvailabilityResponse)
    {
        this.factsApiService
            .Setup(o => o.PriceAvailability(It.IsAny<PriceAvailabilityRequest>()))
            .Returns(priceAvailabilityResponse);
    }

    private void WhenPriceAvailabilityThrowsException(Exception exception)
    {
        this.factsApiService
            .Setup(o => o.PriceAvailability(It.IsAny<PriceAvailabilityRequest>()))
            .Throws(exception);
    }

    private void VerifyPriceAvailabilityWasCalled(Times times)
    {
        this.factsApiService.Verify(
            o => o.PriceAvailability(It.IsAny<PriceAvailabilityRequest>()),
            times
        );
    }
}
