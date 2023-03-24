namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderLoad;

using System;
using System.Collections.Generic;
using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.Services;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests : BaseForPipeTests<OrderLoadParameter, OrderLoadResult>
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
    public void Order_Is_800()
    {
        Assert.AreEqual(800, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_OrderLoadResponse()
    {
        var orderLoadRequest = new OrderLoadRequest();
        var orderLoadResponse = this.CreateOrderLoadResponse(string.Empty, string.Empty);

        this.WhenOrderLoadIs(orderLoadRequest, orderLoadResponse);

        var result = new OrderLoadResult { OrderLoadRequest = orderLoadRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(orderLoadResponse, result.OrderLoadResponse);
    }

    [Test]
    public void Execute_Should_Return_Error_When_OrderLoad_Throws_Exception()
    {
        var orderLoadRequest = new OrderLoadRequest();
        var exception = new Exception("Test Exception Message");

        this.WhenOrderLoadThrowsException(orderLoadRequest, exception);

        var result = new OrderLoadResult { OrderLoadRequest = orderLoadRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(exception.Message, result.Message);
    }

    [Test]
    public void Execute_Should_Return_Error_When_CompleteCode_Is_E()
    {
        var orderLoadRequest = new OrderLoadRequest();
        var orderLoadResponse = this.CreateOrderLoadResponse("E", string.Empty);

        this.WhenOrderLoadIs(orderLoadRequest, orderLoadResponse);

        var result = new OrderLoadResult { OrderLoadRequest = orderLoadRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
    }

    [Test]
    public void Execute_Should_Return_Error_When_Message_Is_Populated()
    {
        var orderLoadRequest = new OrderLoadRequest();
        var orderLoadResponse = this.CreateOrderLoadResponse(string.Empty, "TheMessage");

        this.WhenOrderLoadIs(orderLoadRequest, orderLoadResponse);

        var result = new OrderLoadResult { OrderLoadRequest = orderLoadRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
    }

    private OrderLoadResponse CreateOrderLoadResponse(string completionCode, string message)
    {
        return new OrderLoadResponse
        {
            Response = new Response
            {
                Orders = new List<ResponseOrder>
                {
                    new ResponseOrder { CompletionCode = completionCode, Message = message }
                }
            }
        };
    }

    private void WhenOrderLoadIs(
        OrderLoadRequest orderLoadRequest,
        OrderLoadResponse orderLoadResponse
    )
    {
        this.factsApiService.Setup(o => o.OrderLoad(orderLoadRequest)).Returns(orderLoadResponse);
    }

    private void WhenOrderLoadThrowsException(
        OrderLoadRequest orderLoadRequest,
        Exception exception
    )
    {
        this.factsApiService.Setup(o => o.OrderLoad(orderLoadRequest)).Throws(exception);
    }
}
