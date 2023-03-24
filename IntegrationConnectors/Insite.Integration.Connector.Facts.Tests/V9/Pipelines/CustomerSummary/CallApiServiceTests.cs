namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.CustomerSummary;

using System;
using Insite.Common.Dependencies;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.Services;
using Insite.Integration.Connector.Facts.V9.Api.Models.CustomerSummary;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.CustomerSummary;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CallApiServiceTests : BaseForPipeTests<CustomerSummaryParameter, CustomerSummaryResult>
{
    private Mock<IFactsApiService> factsApiService;

    public override Type PipeType => typeof(CallApiService);

    public override void SetUp()
    {
        this.factsApiService = this.container.GetMock<IFactsApiService>();

        var factsApiServiceFactory = this.container.GetMock<IFactsApiServiceFactory>();
        factsApiServiceFactory
            .Setup(o => o.GetFactsApiService(null))
            .Returns(this.factsApiService.Object);

        var dependencyLocator = this.container.GetMock<IDependencyLocator>();
        dependencyLocator
            .Setup(o => o.GetInstance<IFactsApiServiceFactory>())
            .Returns(factsApiServiceFactory.Object);
    }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_CustomerSummaryResponse()
    {
        var customerSummaryRequest = new CustomerSummaryRequest();
        var customerSummaryResponse = new CustomerSummaryResponse();

        this.WhenCustomerSummaryIs(customerSummaryRequest, customerSummaryResponse);

        var result = new CustomerSummaryResult { CustomerSummaryRequest = customerSummaryRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Success, result.ResultCode);
        Assert.AreEqual(customerSummaryResponse, result.CustomerSummaryResponse);
    }

    [Test]
    public void Execute_Should_Return_Error_When_CustomerSummary_Throws_Exception()
    {
        var customerSummaryRequest = new CustomerSummaryRequest();
        var exception = new Exception("Test Exception Message");

        this.WhenCustomerSummaryThrowsException(customerSummaryRequest, exception);

        var result = new CustomerSummaryResult { CustomerSummaryRequest = customerSummaryRequest };
        result = this.RunExecute(result);

        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(exception.Message, result.Message);
    }

    private void WhenCustomerSummaryIs(
        CustomerSummaryRequest customerSummaryRequest,
        CustomerSummaryResponse customerSummaryResponse
    )
    {
        this.factsApiService
            .Setup(o => o.CustomerSummary(customerSummaryRequest))
            .Returns(customerSummaryResponse);
    }

    private void WhenCustomerSummaryThrowsException(
        CustomerSummaryRequest customerSummaryRequest,
        Exception exception
    )
    {
        this.factsApiService
            .Setup(o => o.CustomerSummary(customerSummaryRequest))
            .Throws(exception);
    }
}
