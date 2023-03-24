namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.CustomerPaymentMethod;

using System;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Acumatica.Services;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.CustomerPaymentMethod;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.CustomerPaymentMethod;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<CustomerPaymentMethodParameter, CustomerPaymentMethodResult>
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
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
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
        var customerPaymentMethodRequest = new CustomerPaymentMethod();
        var customerPaymentMethodResponse = new CustomerPaymentMethod();

        this.WhenCustomerPaymentMethodIs(
            customerPaymentMethodRequest,
            customerPaymentMethodResponse
        );

        var result = this.GetDefaultResult();
        result.CustomerPaymentMethodRequest = customerPaymentMethodRequest;

        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(customerPaymentMethodResponse, result.CustomerPaymentMethodResponse);
    }

    [Test]
    public void Execute_Should_Call_Logout()
    {
        this.RunExecute();

        this.VerifyLogoutWasCalled();
    }

    protected void WhenCustomerPaymentMethodIs(
        CustomerPaymentMethod customerPaymentMethodRequest,
        CustomerPaymentMethod customerPaymentMethodResponse
    )
    {
        this.acumaticaApiService
            .Setup(o => o.CustomerPaymentMethod(customerPaymentMethodRequest))
            .Returns(customerPaymentMethodResponse);
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
