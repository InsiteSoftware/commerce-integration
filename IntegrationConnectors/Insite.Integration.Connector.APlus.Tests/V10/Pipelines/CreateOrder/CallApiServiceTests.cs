namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.CreateOrder;

using System;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;
using Insite.Integration.Connector.APlus.Services;

[TestFixture]
public class CallApiServiceTests : BaseForPipeTests<CreateOrderParameter, CreateOrderResult>
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
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Response()
    {
        var createOrderRequest = new CreateOrderRequest();
        var createOrderResponse = new CreateOrderResponse();

        this.WhenCreateOrderIs(createOrderRequest, createOrderResponse);

        var result = new CreateOrderResult() { CreateOrderRequest = createOrderRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(createOrderResponse, result.CreateOrderResponse);
    }

    [Test]
    public void Execute_Should_Return_Error_When_AccountsReceivableSummary_Throws_Exception()
    {
        var createOrderRequest = new CreateOrderRequest();
        var exceptionMessage = "error message";

        this.WhenCreateOrderThrowsException(createOrderRequest, exceptionMessage);

        var result = new CreateOrderResult() { CreateOrderRequest = createOrderRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, exceptionMessage);
    }

    protected void WhenCreateOrderIs(
        CreateOrderRequest createOrderRequest,
        CreateOrderResponse createOrderResponse
    )
    {
        this.aPlusService
            .Setup(o => o.CreateOrder(createOrderRequest))
            .Returns(createOrderResponse);
    }

    protected void WhenCreateOrderThrowsException(
        CreateOrderRequest createOrderRequest,
        string exceptionMessage
    )
    {
        this.aPlusService
            .Setup(o => o.CreateOrder(createOrderRequest))
            .Throws(new Exception(exceptionMessage));
    }
}
