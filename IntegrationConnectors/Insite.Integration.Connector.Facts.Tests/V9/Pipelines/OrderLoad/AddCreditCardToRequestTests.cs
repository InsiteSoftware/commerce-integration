namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.OrderLoad;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.OrderLoad;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.OrderLoad;
using Insite.PaymentGateway.Cenpos;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AddCreditCardToRequestTests : BaseForPipeTests<OrderLoadParameter, OrderLoadResult>
{
    private Mock<CenposSettings> cenposSettings;

    public override Type PipeType => typeof(AddCreditCardToRequest);

    public override void SetUp()
    {
        this.cenposSettings = this.container.GetMock<CenposSettings>();
    }

    [Test]
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Bypass_Pipe_When_Order_Has_No_CreditCardTransactions()
    {
        var parameter = new OrderLoadParameter { CustomerOrder = Some.CustomerOrder() };

        Assert.DoesNotThrow(() => this.RunExecute(parameter));
    }

    [Test]
    public void Execute_Should_Add_CreditCard_To_OrderHeader()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.CreditCardTransaction()
                    .WithAmount(12)
                    .WithPNRef("835")
                    .WithAuthCode("123ABC")
                    .WithCreditCardNumber("xxxxxxxxxxxx1234")
            )
            .WithTermsCode("CC")
            .Build();

        this.WhenCenposMerchantIdIs(1);

        var parameter = new OrderLoadParameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().Amount,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.AuthorizationAmount
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().PNRef,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.CCRefNum
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().AuthCode,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.CCAuthNum
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().CreditCardNumber,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.CCMaskedNum
        );
        Assert.AreEqual(
            "1",
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.CCMerchantID
        );
        Assert.AreEqual(
            "CENPOS",
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.CCProcessorID
        );
        Assert.AreEqual(
            customerOrder.TermsCode,
            result.OrderLoadRequest.Request.Orders.First().OrderHeader.CCType
        );
    }

    protected override OrderLoadResult GetDefaultResult()
    {
        return new OrderLoadResult
        {
            OrderLoadRequest = new OrderLoadRequest
            {
                Request = new Request
                {
                    Orders = new List<RequestOrder>
                    {
                        new RequestOrder { OrderHeader = new OrderHeader() }
                    }
                }
            }
        };
    }

    private void WhenCenposMerchantIdIs(int merchantId)
    {
        this.cenposSettings.Setup(o => o.MerchantId).Returns(merchantId);
    }
}
