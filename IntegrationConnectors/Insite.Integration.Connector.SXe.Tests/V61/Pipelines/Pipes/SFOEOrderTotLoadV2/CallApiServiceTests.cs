namespace Insite.Integration.Connector.SXe.Tests.V6.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;
using Insite.Integration.Connector.SXe.V61;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    private Mock<ISXeApiServiceV61> sxeApiService;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.sxeApiService = this.container.GetMock<ISXeApiServiceV61>();

        var sxeApiServiceFactory = this.container.GetMock<ISXeApiServiceFactory>();
        sxeApiServiceFactory
            .Setup(o => o.GetSXeApiServiceV61(null))
            .Returns(this.sxeApiService.Object);

        var dependencyLocator = this.container.GetMock<IDependencyLocator>();
        dependencyLocator
            .Setup(o => o.GetInstance<ISXeApiServiceFactory>())
            .Returns(sxeApiServiceFactory.Object);
    }

    [Test]
    public void Order_Is_900()
    {
        Assert.AreEqual(900, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Return_Error_When_SFOEOrderTotLoadV2_Returns_Error()
    {
        var sFOEOrderTotLoadV2Request = new SFOEOrderTotLoadV2Request();
        var sFOEOrderTotLoadV2Response = new SFOEOrderTotLoadV2Response { errorMessage = "ABC123" };

        this.WhenSFOEOrderTotLoadV2Is(sFOEOrderTotLoadV2Request, sFOEOrderTotLoadV2Response);

        var result = new SFOEOrderTotLoadV2Result
        {
            SFOEOrderTotLoadV2Request = sFOEOrderTotLoadV2Request
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, sFOEOrderTotLoadV2Response.errorMessage);
        Assert.AreEqual(sFOEOrderTotLoadV2Response, result.SFOEOrderTotLoadV2Response);
    }

    [Test]
    public void Execute_Should_Get_SFOEOrderTotLoadV2Response()
    {
        var sFOEOrderTotLoadV2Request = new SFOEOrderTotLoadV2Request();
        var sFOEOrderTotLoadV2Response = new SFOEOrderTotLoadV2Response();

        this.WhenSFOEOrderTotLoadV2Is(sFOEOrderTotLoadV2Request, sFOEOrderTotLoadV2Response);

        var result = new SFOEOrderTotLoadV2Result
        {
            SFOEOrderTotLoadV2Request = sFOEOrderTotLoadV2Request
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(sFOEOrderTotLoadV2Response, result.SFOEOrderTotLoadV2Response);
    }

    protected void WhenSFOEOrderTotLoadV2Is(
        SFOEOrderTotLoadV2Request sFOEOrderTotLoadV2Request,
        SFOEOrderTotLoadV2Response sFOEOrderTotLoadV2Response
    )
    {
        this.sxeApiService
            .Setup(o => o.SFOEOrderTotLoadV2(sFOEOrderTotLoadV2Request))
            .Returns(sFOEOrderTotLoadV2Response);
    }
}
