namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using Insite.Common.Dependencies;
using Moq;
using NUnit.Framework;

using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V11;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    private Mock<ISXeApiServiceV11> sxeApiService;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.sxeApiService = this.container.GetMock<ISXeApiServiceV11>();

        var sxeApiServiceFactory = this.container.GetMock<ISXeApiServiceFactory>();
        sxeApiServiceFactory
            .Setup(o => o.GetSXeApiServiceV11(null))
            .Returns(this.sxeApiService.Object);

        var dependencyLocator = this.container.GetMock<IDependencyLocator>();
        dependencyLocator
            .Setup(o => o.GetInstance<ISXeApiServiceFactory>())
            .Returns(sxeApiServiceFactory.Object);
    }

    [Test]
    public void Order_Is_1000()
    {
        Assert.AreEqual(1000, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Return_Error_When_SFOEOrderTotLoadV4_Returns_Error()
    {
        var sFOEOrderTotLoadV4Request = new SFOEOrderTotLoadV4Request();
        var sFOEOrderTotLoadV4Response = new SFOEOrderTotLoadV4Response
        {
            Response = new Response { ErrorMessage = "ABC123" }
        };

        this.WhenSFOEOrderTotLoadV4Is(sFOEOrderTotLoadV4Request, sFOEOrderTotLoadV4Response);

        var result = new SFOEOrderTotLoadV4Result
        {
            SFOEOrderTotLoadV4Request = sFOEOrderTotLoadV4Request
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, sFOEOrderTotLoadV4Response.Response.ErrorMessage);
        Assert.AreEqual(sFOEOrderTotLoadV4Response, result.SFOEOrderTotLoadV4Response);
    }

    [Test]
    public void Execute_Should_Get_SFOEOrderTotLoadV4Response()
    {
        var sFOEOrderTotLoadV4Request = new SFOEOrderTotLoadV4Request();
        var sFOEOrderTotLoadV4Response = new SFOEOrderTotLoadV4Response();

        this.WhenSFOEOrderTotLoadV4Is(sFOEOrderTotLoadV4Request, sFOEOrderTotLoadV4Response);

        var result = new SFOEOrderTotLoadV4Result
        {
            SFOEOrderTotLoadV4Request = sFOEOrderTotLoadV4Request
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(sFOEOrderTotLoadV4Response, result.SFOEOrderTotLoadV4Response);
    }

    protected void WhenSFOEOrderTotLoadV4Is(
        SFOEOrderTotLoadV4Request sFOEOrderTotLoadV4Request,
        SFOEOrderTotLoadV4Response sFOEOrderTotLoadV4Response
    )
    {
        this.sxeApiService
            .Setup(o => o.SFOEOrderTotLoadV4(sFOEOrderTotLoadV4Request))
            .Returns(sFOEOrderTotLoadV4Response);
    }
}
