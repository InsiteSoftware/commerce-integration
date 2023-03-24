namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderTotal;

using System;
using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.Services;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderTotal;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderTotal;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests : BaseForPipeTests<OrderTotalParameter, OrderTotalResult>
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
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_OrderTotalResponse()
    {
        var orderTotalRequest = new OrderTotalRequest();
        var orderTotalResponse = new OrderTotalResponse();

        this.WhenOrderTotalIs(orderTotalRequest, orderTotalResponse);

        var result = new OrderTotalResult { OrderTotalRequest = orderTotalRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(orderTotalResponse, result.OrderTotalResponse);
    }

    [Test]
    public void Execute_Should_Return_Error_When_OrderTotal_Throws_Exception()
    {
        var orderTotalRequest = new OrderTotalRequest();
        var exception = new Exception("Test Exception Message");

        this.WhenOrderTotalThrowsException(orderTotalRequest, exception);

        var result = new OrderTotalResult { OrderTotalRequest = orderTotalRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(exception.Message, result.Message);
    }

    private void WhenOrderTotalIs(
        OrderTotalRequest orderTotalRequest,
        OrderTotalResponse orderTotalResponse
    )
    {
        this.factsApiService
            .Setup(o => o.OrderTotal(orderTotalRequest))
            .Returns(orderTotalResponse);
    }

    private void WhenOrderTotalThrowsException(
        OrderTotalRequest orderTotalRequest,
        Exception exception
    )
    {
        this.factsApiService.Setup(o => o.OrderTotal(orderTotalRequest)).Throws(exception);
    }
}
