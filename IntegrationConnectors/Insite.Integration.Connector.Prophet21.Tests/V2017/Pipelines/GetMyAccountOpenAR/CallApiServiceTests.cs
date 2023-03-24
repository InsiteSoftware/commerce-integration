namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetMyAccountOpenAR;

using System;

using Moq;
using NUnit.Framework;

using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetMyAccountOpenAR;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetMyAccountOpenAR;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Insite.Integration.Connector.Prophet21.Services;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<GetMyAccountOpenARParameter, GetMyAccountOpenARResult>
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
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_GetMyAccountOpenAR_Reply()
    {
        var getMyAccountOpenARRequest = new GetMyAccountOpenAR();
        var getMyAccountOpenARReply = new GetMyAccountOpenAR
        {
            ReplyStatus = new ReplyStatus { Result = 0 }
        };

        this.WhenGetMyAccountOpenARIs(getMyAccountOpenARRequest, getMyAccountOpenARReply);

        var result = new GetMyAccountOpenARResult
        {
            GetMyAccountOpenARRequest = getMyAccountOpenARRequest
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(getMyAccountOpenARReply, result.GetMyAccountOpenARReply);
    }

    [Test]
    public void Execute_Should_Return_Error_When_GetItemPrice_Returns_Error()
    {
        var getMyAccountOpenARRequest = new GetMyAccountOpenAR();
        var getMyAccountOpenARReply = new GetMyAccountOpenAR
        {
            ReplyStatus = new ReplyStatus { Result = 1, Message = "Error Message" }
        };

        this.WhenGetMyAccountOpenARIs(getMyAccountOpenARRequest, getMyAccountOpenARReply);

        var result = new GetMyAccountOpenARResult
        {
            GetMyAccountOpenARRequest = getMyAccountOpenARRequest
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, "Error Message");
    }

    protected void WhenGetMyAccountOpenARIs(
        GetMyAccountOpenAR getMyAccountOpenARRequest,
        GetMyAccountOpenAR getMyAccountOpenARReply
    )
    {
        this.prophet21ApiService
            .Setup(o => o.GetMyAccountOpenAR(null, getMyAccountOpenARRequest))
            .Returns(getMyAccountOpenARReply);
    }
}
