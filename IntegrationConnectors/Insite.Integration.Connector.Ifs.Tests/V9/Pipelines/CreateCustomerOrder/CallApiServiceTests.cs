namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.CreateCustomerOrder;

using System;

using Moq;
using NUnit.Framework;

using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.Services;

[TestFixture]
public class CallApiServiceTests
    : BaseForPipeTests<CreateCustomerOrderParameter, CreateCustomerOrderResult>
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
    public void Order_Is_700()
    {
        Assert.AreEqual(700, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_OrderResponse()
    {
        var customerOrder = new customerOrder();
        var orderResponse = new orderResponse();

        this.WhenCreateCustomerOrderIs(customerOrder, orderResponse);

        var result = this.GetDefaultResult();
        result.CustomerOrder = customerOrder;

        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(orderResponse, result.OrderResponse);
    }

    [Test]
    public void Execute_Should_Return_Error_When_CreateCustomerOrder_Returns_Error()
    {
        var customerOrder = new customerOrder();
        var orderResponse = new orderResponse { errorMessage = "Error Message" };

        this.WhenCreateCustomerOrderIs(customerOrder, orderResponse);

        var result = this.GetDefaultResult();
        result.CustomerOrder = customerOrder;

        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(orderResponse.errorMessage, result.Message);
    }

    protected void WhenCreateCustomerOrderIs(
        customerOrder customerOrder,
        orderResponse orderResponse
    )
    {
        this.ifsApiService.Setup(o => o.CreateCustomerOrder(customerOrder)).Returns(orderResponse);
    }
}
