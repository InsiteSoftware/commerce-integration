namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.GetCustomerPrice;

using System;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Ifs;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Integration.Connector.Ifs.Services;
using Insite.Core.Services;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<GetCustomerPriceParameter, GetCustomerPriceResult>
{
    private Mock<IIfsApiService> ifsApiService;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.ifsApiService = this.container.GetMock<IIfsApiService>();

        var ifsApiServiceFactory = this.container.GetMock<IIfsApiServiceFactory>();
        ifsApiServiceFactory
            .Setup(o => o.GetIfsApiService(null))
            .Returns(this.ifsApiService.Object);

        var dependencyLocator = this.container.GetMock<IDependencyLocator>();
        dependencyLocator
            .Setup(o => o.GetInstance<IIfsApiServiceFactory>())
            .Returns(ifsApiServiceFactory.Object);
    }

    [Test]
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_CustomerPriceResponse()
    {
        var customerPriceRequest = new customerPriceRequest();
        var customerPriceResponse = new customerPriceResponse { errorText = string.Empty };

        this.WhenGetCustomerPriceIs(customerPriceRequest, customerPriceResponse);

        var result = new GetCustomerPriceResult { CustomerPriceRequest = customerPriceRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(customerPriceResponse, result.CustomerPriceResponse);
    }

    [Test]
    public void Execute_Should_Return_Error_When_GetCustomerPrice_Returns_Error()
    {
        var customerPriceRequest = new customerPriceRequest();
        var customerPriceResponse = new customerPriceResponse { errorText = "Error Message" };

        this.WhenGetCustomerPriceIs(customerPriceRequest, customerPriceResponse);

        var result = new GetCustomerPriceResult { CustomerPriceRequest = customerPriceRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(customerPriceResponse.errorText, result.Message);
    }

    protected void WhenGetCustomerPriceIs(
        customerPriceRequest customerPriceRequest,
        customerPriceResponse customerPriceResponse
    )
    {
        this.ifsApiService
            .Setup(o => o.GetCustomerPrice(customerPriceRequest))
            .Returns(customerPriceResponse);
    }
}
