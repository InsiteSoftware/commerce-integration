namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetCartSummary;

using System;

using Moq;
using NUnit.Framework;

using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetCartSummary;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Integration.Connector.Prophet21.Services;

[TestFixture]
public class CallApiServiceTests : BaseForPipeTests<GetCartSummaryParameter, GetCartSummaryResult>
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
    public void Execute_Should_Get_GetCartSummary_Reply()
    {
        var getCartSummaryRequest = new GetCartSummary();
        var getCartSummaryReply = new GetCartSummary()
        {
            ReplyStatus = new ReplyStatus { Result = 0 }
        };

        this.WhenGetCartSummaryIs(getCartSummaryRequest, getCartSummaryReply);

        var result = new GetCartSummaryResult() { GetCartSummaryRequest = getCartSummaryRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(getCartSummaryReply, result.GetCartSummaryReply);
    }

    [Test]
    public void Execute_Should_Return_Error_When_GetCartSummary_Returns_Error()
    {
        var getCartSummaryRequest = new GetCartSummary();
        var getCartSummaryReply = new GetCartSummary()
        {
            ReplyStatus = new ReplyStatus { Result = 1, Message = "Error Message" }
        };

        this.WhenGetCartSummaryIs(getCartSummaryRequest, getCartSummaryReply);

        var result = new GetCartSummaryResult() { GetCartSummaryRequest = getCartSummaryRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, "Error Message");
    }

    protected void WhenGetCartSummaryIs(
        GetCartSummary getCartSummaryRequest,
        GetCartSummary getCartSummaryReply
    )
    {
        this.prophet21ApiService
            .Setup(o => o.GetCartSummary(null, getCartSummaryRequest))
            .Returns(getCartSummaryReply);
    }
}
