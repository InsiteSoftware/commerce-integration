namespace Insite.Integration.Connector.SXe.Tests.V10.Pipelines.Pipes.OEPricingMultipleV4;

using System;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.OEPricingMultipleV4;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.OEPricingMultipleV4;
using Insite.Integration.Connector.SXe.V10;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    private Mock<ISXeApiServiceV10> sxeApiService;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.sxeApiService = this.container.GetMock<ISXeApiServiceV10>();

        var sxeApiServiceFactory = this.container.GetMock<ISXeApiServiceFactory>();
        sxeApiServiceFactory
            .Setup(o => o.GetSXeApiServiceV10(null))
            .Returns(this.sxeApiService.Object);

        var dependencyLocator = this.container.GetMock<IDependencyLocator>();
        dependencyLocator
            .Setup(o => o.GetInstance<ISXeApiServiceFactory>())
            .Returns(sxeApiServiceFactory.Object);
    }

    [Test]
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Return_Error_Result_When_OEPricingMultipleV4_Returns_Error_Message()
    {
        var oePricingMultipleV4Request = new OEPricingMultipleV4Request();
        var oePricingMultipleV4Response = new OEPricingMultipleV4Response
        {
            ErrorMessage = "ABC123"
        };

        this.WhenOEPricingMultipleV4Is(oePricingMultipleV4Request, oePricingMultipleV4Response);

        var result = new OEPricingMultipleV4Result
        {
            OEPricingMultipleV4Request = oePricingMultipleV4Request
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, oePricingMultipleV4Response.ErrorMessage);
        Assert.AreEqual(oePricingMultipleV4Response, result.OEPricingMultipleV4Response);
    }

    [Test]
    public void Execute_Should_Get_OEPricingMultipleV4Response()
    {
        var oePricingMultipleV4Request = new OEPricingMultipleV4Request();
        var oePricingMultipleV4Response = new OEPricingMultipleV4Response();

        this.WhenOEPricingMultipleV4Is(oePricingMultipleV4Request, oePricingMultipleV4Response);

        var result = new OEPricingMultipleV4Result
        {
            OEPricingMultipleV4Request = oePricingMultipleV4Request
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(oePricingMultipleV4Response, result.OEPricingMultipleV4Response);
    }

    protected void WhenOEPricingMultipleV4Is(
        OEPricingMultipleV4Request oePricingMultipleV4Request,
        OEPricingMultipleV4Response oePricingMultipleV4Response
    )
    {
        this.sxeApiService
            .Setup(o => o.OEPricingMultipleV4(oePricingMultipleV4Request))
            .Returns(oePricingMultipleV4Response);
    }
}
