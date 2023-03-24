namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.SalesOrder;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;
using Moq;
using NUnit.Framework;

[TestFixture]
public class SubmitCustomerPaymentMethodTests
    : BaseForPipeTests<SalesOrderParameter, SalesOrderResult>
{
    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    public override Type PipeType => typeof(SubmitCustomerPaymentMethod);

    public override void SetUp()
    {
        this.pipeAssemblyFactory = this.container.GetMock<IPipeAssemblyFactory>();
    }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Bypass_Handler_When_CustomerOrder_Has_No_CreditCardTransactions()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = Some.CustomerOrder();

        var result = this.RunExecute(parameter);

        this.VerifyExecutePipelineWasNotCalled();
    }

    [Test]
    public void Execute_Should_Return_Error_When_CustomerPaymentMethod_Returns_Error()
    {
        var customerPaymentMethodResult = new CustomerPaymentMethodResult
        {
            ResultCode = ResultCode.Error,
            SubCode = SubCode.GeneralFailure,
            Messages = new List<ResultMessage> { new ResultMessage { Message = "Error" } }
        };

        var customerOrder = Some.CustomerOrder().With(Some.CreditCardTransaction()).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenExecutePipelineIs(
            customerOrder.CreditCardTransactions.First(),
            customerPaymentMethodResult
        );

        var result = this.RunExecute(parameter);

        this.VerifyExecutePipelineWasCalled(customerOrder.CreditCardTransactions.First());
        Assert.AreEqual(ResultCode.Error, result.ResultCode);
        Assert.AreEqual(SubCode.GeneralFailure, result.SubCode);
        Assert.AreEqual(customerPaymentMethodResult.Message, result.Message);
    }

    [Test]
    public void Execute_Should_Populate_Payment_Info_When_CustomerPaymentMethod_Is_Successful()
    {
        var customerPaymentMethodResult = new CustomerPaymentMethodResult
        {
            ResultCode = ResultCode.Success,
            SubCode = SubCode.Success,
            CustomerProfileId = "Erp123"
        };

        var customerOrder = Some.CustomerOrder()
            .With(
                Some.CreditCardTransaction()
                    .WithToken2("Token2Value")
                    .WithPNRef("PNRefValue")
                    .WithAmount(12)
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenExecutePipelineIs(
            customerOrder.CreditCardTransactions.First(),
            customerPaymentMethodResult
        );

        var result = this.RunExecute(parameter);

        this.VerifyExecutePipelineWasCalled(customerOrder.CreditCardTransactions.First());
        Assert.AreEqual(
            customerPaymentMethodResult.CustomerProfileId,
            result.SalesOrderRequest.PaymentCardIdentifier
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().Token2,
            result.SalesOrderRequest.PaymentProfileID
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().PNRef,
            result.SalesOrderRequest.PreAuthorizationNbr
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().Amount,
            result.SalesOrderRequest.PreAuthorizedAmount
        );
    }

    protected void WhenExecutePipelineIs(
        CreditCardTransaction creditCardTransaction,
        CustomerPaymentMethodResult customerPaymentMethodResult
    )
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.Is<CustomerPaymentMethodParameter>(
                            p => p.CreditCardTransaction == creditCardTransaction
                        ),
                        It.IsAny<CustomerPaymentMethodResult>()
                    )
            )
            .Returns(customerPaymentMethodResult);
    }

    protected void VerifyExecutePipelineWasNotCalled()
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.IsAny<CustomerPaymentMethodParameter>(),
                    It.IsAny<CustomerPaymentMethodResult>()
                ),
            Times.Never
        );
    }

    protected void VerifyExecutePipelineWasCalled(CreditCardTransaction creditCardTransaction)
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<CustomerPaymentMethodParameter>(
                        p => p.CreditCardTransaction == creditCardTransaction
                    ),
                    It.IsAny<CustomerPaymentMethodResult>()
                ),
            Times.Once
        );
    }

    protected override SalesOrderResult GetDefaultResult()
    {
        return new SalesOrderResult { SalesOrderRequest = new SalesOrder() };
    }
}
