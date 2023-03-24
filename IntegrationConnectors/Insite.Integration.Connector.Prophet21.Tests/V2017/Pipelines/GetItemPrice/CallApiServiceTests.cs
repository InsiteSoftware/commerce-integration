namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetItemPrice;

using System;

using Moq;
using NUnit.Framework;

using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Integration.Connector.Prophet21.Services;

[TestFixture]
public class CallApiServiceTests : BaseForPipeTests<GetItemPriceParameter, GetItemPriceResult>
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
    public void Order_Is_700()
    {
        Assert.AreEqual(700, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_GetItemPrice_Reply()
    {
        var getItemPriceRequest = new GetItemPrice();
        var getItemPriceReply = new GetItemPrice { ReplyStatus = new ReplyStatus { Result = 0 } };

        this.WhenGetItemPriceIs(getItemPriceRequest, getItemPriceReply);

        var result = new GetItemPriceResult { GetItemPriceRequest = getItemPriceRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(getItemPriceReply, result.GetItemPriceReply);
    }

    [Test]
    public void Execute_Should_Return_Error_When_GetItemPrice_Returns_Error()
    {
        var getItemPriceRequest = new GetItemPrice();
        var getItemPriceReply = new GetItemPrice
        {
            ReplyStatus = new ReplyStatus { Result = 1, Message = "Error Message" }
        };

        this.WhenGetItemPriceIs(getItemPriceRequest, getItemPriceReply);

        var result = new GetItemPriceResult { GetItemPriceRequest = getItemPriceRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, "Error Message");
    }

    protected void WhenGetItemPriceIs(
        GetItemPrice getItemPriceRequest,
        GetItemPrice getItemPriceReply
    )
    {
        this.prophet21ApiService
            .Setup(o => o.GetItemPrice(null, getItemPriceRequest))
            .Returns(getItemPriceReply);
    }
}
