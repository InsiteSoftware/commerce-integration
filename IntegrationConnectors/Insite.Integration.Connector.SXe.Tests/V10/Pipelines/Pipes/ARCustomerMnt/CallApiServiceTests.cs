namespace Insite.Integration.Connector.SXe.Tests.V10.Pipelines.Pipes.ARCustomerMnt;

using System;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V10;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.ARCustomerMntRequest;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.ARCustomerMnt;

[TestFixture]
public class CallApiServiceTests : BaseForPipeTests<ARCustomerMntParameter, ARCustomerMntResult>
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
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Return_Error_When_OEFullOrderMntV6_Returns_Error_And_Return_Data_Is_Not_Successful()
    {
        var arCustomerMntRequest = new ARCustomerMntRequest();
        var arCustomerMntResponse = new ARCustomerMntResponse { ErrorMessage = "error message" };

        this.WhenARCustomerMntIs(arCustomerMntRequest, arCustomerMntResponse);

        var result = new ARCustomerMntResult { ARCustomerMntRequest = arCustomerMntRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(result.Message, arCustomerMntResponse.ErrorMessage);
        Assert.AreEqual(arCustomerMntResponse, result.ARCustomerMntResponse);
    }

    [Test]
    public void Execute_Should_Return_Success_When_OEFullOrderMntV6_Returns_Error_But_Return_Data_Is_Successful()
    {
        var arCustomerMntRequest = new ARCustomerMntRequest();
        var arCustomerMntResponse = new ARCustomerMntResponse
        {
            ErrorMessage = "error message",
            ReturnData = "abc Update Successful 123"
        };

        this.WhenARCustomerMntIs(arCustomerMntRequest, arCustomerMntResponse);

        var result = new ARCustomerMntResult { ARCustomerMntRequest = arCustomerMntRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(arCustomerMntResponse, result.ARCustomerMntResponse);
    }

    [Test]
    public void Execute_Should_Get_ARCustomerMntResponse()
    {
        var arCustomerMntRequest = new ARCustomerMntRequest();
        var arCustomerMntResponse = new ARCustomerMntResponse();

        this.WhenARCustomerMntIs(arCustomerMntRequest, arCustomerMntResponse);

        var result = new ARCustomerMntResult { ARCustomerMntRequest = arCustomerMntRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(arCustomerMntResponse, result.ARCustomerMntResponse);
    }

    protected void WhenARCustomerMntIs(
        ARCustomerMntRequest arCustomerMntRequest,
        ARCustomerMntResponse arCustomerMntResponse
    )
    {
        this.sxeApiService
            .Setup(o => o.ARCustomerMnt(arCustomerMntRequest))
            .Returns(arCustomerMntResponse);
    }
}
