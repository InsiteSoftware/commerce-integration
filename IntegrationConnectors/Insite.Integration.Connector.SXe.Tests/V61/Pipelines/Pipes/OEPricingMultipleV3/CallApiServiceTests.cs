namespace Insite.Integration.Connector.SXe.Tests.V6.Pipelines.Pipes.OEPricingMultipleV3;

using System;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.OEPricingMultipleV3;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;
using Insite.Integration.Connector.SXe.V61;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<OEPricingMultipleV3Parameter, OEPricingMultipleV3Result>
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
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Return_Error_Result_When_OEPricingMultipleV3_Returns_Error_Message()
    {
        var oePricingMultipleV3Request = new OEPricingMultipleV3Request();
        var oePricingMultipleV3Response = new OEPricingMultipleV3Response
        {
            errorMessage = "ABC123"
        };

        this.WhenOEPricingMultipleV3Is(oePricingMultipleV3Request, oePricingMultipleV3Response);

        var result = new OEPricingMultipleV3Result
        {
            OEPricingMultipleV3Request = oePricingMultipleV3Request
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, oePricingMultipleV3Response.errorMessage);
        Assert.AreEqual(oePricingMultipleV3Response, result.OEPricingMultipleV3Response);
    }

    [Test]
    public void Execute_Should_Get_OEPricingMultipleV3Response()
    {
        var oePricingMultipleV3Request = new OEPricingMultipleV3Request();
        var oePricingMultipleV3Response = new OEPricingMultipleV3Response();

        this.WhenOEPricingMultipleV3Is(oePricingMultipleV3Request, oePricingMultipleV3Response);

        var result = new OEPricingMultipleV3Result
        {
            OEPricingMultipleV3Request = oePricingMultipleV3Request
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(oePricingMultipleV3Response, result.OEPricingMultipleV3Response);
    }

    protected void WhenOEPricingMultipleV3Is(
        OEPricingMultipleV3Request oePricingMultipleV3Request,
        OEPricingMultipleV3Response oePricingMultipleV3Response
    )
    {
        this.sxeApiService
            .Setup(o => o.OEPricingMultipleV3(oePricingMultipleV3Request))
            .Returns(oePricingMultipleV3Response);
    }
}
