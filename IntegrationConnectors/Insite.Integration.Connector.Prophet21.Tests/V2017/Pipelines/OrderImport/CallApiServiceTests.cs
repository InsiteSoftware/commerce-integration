namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.OrderImport;

using System;

using Moq;
using NUnit.Framework;

using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Integration.Connector.Prophet21.Services;

[TestFixture]
public class CallApiServiceTests : BaseForPipeTests<OrderImportParameter, OrderImportResult>
{
    private Mock<IProphet21ApiService> prophet21ApiService;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.prophet21ApiService = this.container.GetMock<IProphet21ApiService>();

        this.dependencyLocator
            .Setup(o => o.GetInstance<IProphet21ApiService>())
            .Returns(this.prophet21ApiService.Object);
    }

    [Test]
    public void Order_Is_900()
    {
        Assert.AreEqual(900, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_GetItemPrice_Reply()
    {
        var orderImportRequest = new OrderImport();
        var orderImportReply = new OrderImport() { ReplyStatus = new ReplyStatus { Result = 0 } };

        this.WhenOrderImportIs(orderImportRequest, orderImportReply);

        var result = new OrderImportResult() { OrderImportRequest = orderImportRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(orderImportReply, result.OrderImportReply);
    }

    [Test]
    public void Execute_Should_Return_Error_When_GetItemPrice_Returns_Error()
    {
        var orderImportRequest = new OrderImport();
        var orderImportReply = new OrderImport()
        {
            ReplyStatus = new ReplyStatus { Result = 1, Message = "Error Message" }
        };

        this.WhenOrderImportIs(orderImportRequest, orderImportReply);

        var result = new OrderImportResult() { OrderImportRequest = orderImportRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, "Error Message");
    }

    protected void WhenOrderImportIs(OrderImport orderImportRequest, OrderImport orderImportReply)
    {
        this.prophet21ApiService
            .Setup(o => o.OrderImport(null, orderImportRequest))
            .Returns(orderImportReply);
    }
}
