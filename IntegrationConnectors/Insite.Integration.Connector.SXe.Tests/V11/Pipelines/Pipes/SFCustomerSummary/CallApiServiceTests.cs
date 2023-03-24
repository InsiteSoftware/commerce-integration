namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.SFCustomerSummary;

using System;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFCustomerSummary;
using Insite.Integration.Connector.SXe.V11;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<SFCustomerSummaryParameter, SFCustomerSummaryResult>
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
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Return_Error_When_SFCustomerSummary_Returns_Error()
    {
        var sfCustomerSummaryRequest = new SFCustomerSummaryRequest();
        var sfCustomerSummaryResponse = new SFCustomerSummaryResponse
        {
            Response = new Response { ErrorMessage = "ABC123" }
        };

        this.WhenSFCustomerSummaryIs(sfCustomerSummaryRequest, sfCustomerSummaryResponse);

        var result = new SFCustomerSummaryResult
        {
            SFCustomerSummaryRequest = sfCustomerSummaryRequest
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, sfCustomerSummaryResponse.Response.ErrorMessage);
        Assert.AreEqual(sfCustomerSummaryResponse, result.SFCustomerSummaryResponse);
    }

    [Test]
    public void Execute_Should_Get_SFCustomerSummaryResponse()
    {
        var sfCustomerSummaryRequest = new SFCustomerSummaryRequest();
        var sfCustomerSummaryResponse = new SFCustomerSummaryResponse();

        this.WhenSFCustomerSummaryIs(sfCustomerSummaryRequest, sfCustomerSummaryResponse);

        var result = new SFCustomerSummaryResult
        {
            SFCustomerSummaryRequest = sfCustomerSummaryRequest
        };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(sfCustomerSummaryResponse, result.SFCustomerSummaryResponse);
    }

    protected void WhenSFCustomerSummaryIs(
        SFCustomerSummaryRequest sfCustomerSummaryRequest,
        SFCustomerSummaryResponse sfCustomerSummaryResponse
    )
    {
        this.sxeApiService
            .Setup(o => o.SFCustomerSummary(sfCustomerSummaryRequest))
            .Returns(sfCustomerSummaryResponse);
    }
}
