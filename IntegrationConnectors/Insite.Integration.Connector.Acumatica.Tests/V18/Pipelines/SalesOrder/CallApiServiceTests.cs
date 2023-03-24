namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.SalesOrder;

using System;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Acumatica.Services;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests : BaseForPipeTests<SalesOrderParameter, SalesOrderResult>
{
    private Mock<IAcumaticaApiService> acumaticaApiService;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.acumaticaApiService = this.container.GetMock<IAcumaticaApiService>();

        var acumaticaApiServiceFactory = this.container.GetMock<IAcumaticaApiServiceFactory>();
        acumaticaApiServiceFactory
            .Setup(o => o.GetAcumaticaApiService(null))
            .Returns(this.acumaticaApiService.Object);

        this.dependencyLocator
            .Setup(o => o.GetInstance<IAcumaticaApiServiceFactory>())
            .Returns(acumaticaApiServiceFactory.Object);
    }

    [Test]
    public void Order_Is_800()
    {
        Assert.AreEqual(800, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Call_Login()
    {
        this.RunExecute();

        this.VerifyLoginWasCalled();
    }

    [Test]
    public void Execute_Should_Get_SalesOrder_Response()
    {
        var salesOrderRequest = new SalesOrder();
        var salesOrderResponse = new SalesOrder();

        this.WhenSalesOrderIs(salesOrderRequest, salesOrderResponse);

        var result = this.GetDefaultResult();
        result.SalesOrderRequest = salesOrderRequest;

        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(salesOrderResponse, result.SalesOrderResponse);
    }

    [Test]
    public void Execute_Should_Call_Logout()
    {
        this.RunExecute();

        this.VerifyLogoutWasCalled();
    }

    protected void WhenSalesOrderIs(SalesOrder salesOrderRequest, SalesOrder salesOrderResponse)
    {
        this.acumaticaApiService
            .Setup(o => o.SalesOrder(salesOrderRequest))
            .Returns(salesOrderResponse);
    }

    private void VerifyLoginWasCalled()
    {
        this.acumaticaApiService.Verify(o => o.Login(), Times.Once);
    }

    private void VerifyLogoutWasCalled()
    {
        this.acumaticaApiService.Verify(o => o.Logout(), Times.Once);
    }
}
