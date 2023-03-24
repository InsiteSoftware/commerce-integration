namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Linq;

using Moq;
using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFOEOrderTotLoadV4;
using Insite.PaymentGateway.Cenpos;

[TestFixture]
public class AddInInFieldValuesToRequestTests
    : BaseForPipeTests<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    private Mock<CenposSettings> cenposSettings;

    public override Type PipeType => typeof(AddInInFieldValuesToRequest);

    public override void SetUp()
    {
        this.cenposSettings = this.container.GetMock<CenposSettings>();
    }

    [Test]
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Not_Populate_InInFieldValue4s_If_Customer_Order_Does_Not_Have_Any_Credit_Card_Transactions()
    {
        var result = this.RunExecute();

        Assert.IsTrue(
            result.SFOEOrderTotLoadV4Request.Request.InFieldValueCollection == null
                || !result.SFOEOrderTotLoadV4Request.Request.InFieldValueCollection.InFieldValues.Any()
        );
    }

    [Test]
    public void Execute_Should_Not_Populate_InInFieldValue4s_If_Customer_Order_Does_Have_Only_PayPal_Transactions()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.CreditCardTransaction().WithCardType("PayPal"))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.IsEmpty(
            result.SFOEOrderTotLoadV4Request.Request.InFieldValueCollection.InFieldValues
        );
    }

    [Test]
    public void Execute_Should_Populate_InInFieldValue4s_If_Customer_Order_Has_Credit_Card_Transactions()
    {
        var customerOrder = Some.CustomerOrder()
            .WithOrderNumber("Z000123")
            .With(
                Some.CreditCardTransaction()
                    .WithCreditCardNumber("1234")
                    .WithCardType("visa")
                    .WithAuthCode("abc123")
                    .WithPNRef("12345")
                    .WithAmount(12)
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenMerchantIdIs(10);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "10",
            result.SFOEOrderTotLoadV4Request.Request.InFieldValueCollection.InFieldValues
                .FirstOrDefault(o => o.Fieldname.Equals("MerchantID"))
                .Fieldvalue
        );
        Assert.AreEqual(
            "cenpos",
            result.SFOEOrderTotLoadV4Request.Request.InFieldValueCollection.InFieldValues
                .FirstOrDefault(o => o.Fieldname.Equals("PaymentType"))
                .Fieldvalue
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().CreditCardNumber,
            result.SFOEOrderTotLoadV4Request.Request.InFieldValueCollection.InFieldValues
                .FirstOrDefault(o => o.Fieldname.Equals("CardNumber"))
                .Fieldvalue
        );
        Assert.AreEqual(
            "VISA",
            result.SFOEOrderTotLoadV4Request.Request.InFieldValueCollection.InFieldValues
                .FirstOrDefault(o => o.Fieldname.Equals("ProcPaymentType"))
                .Fieldvalue
        );
        Assert.AreEqual(
            null,
            result.SFOEOrderTotLoadV4Request.Request.InFieldValueCollection.InFieldValues
                .FirstOrDefault(o => o.Fieldname.Equals("Token"))
                .Fieldvalue
        );
        Assert.AreEqual(
            "12",
            result.SFOEOrderTotLoadV4Request.Request.InFieldValueCollection.InFieldValues
                .FirstOrDefault(o => o.Fieldname.Equals("AuthAmt"))
                .Fieldvalue
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().PNRef,
            result.SFOEOrderTotLoadV4Request.Request.InFieldValueCollection.InFieldValues
                .FirstOrDefault(o => o.Fieldname.Equals("ReferenceNumber"))
                .Fieldvalue
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().AuthCode,
            result.SFOEOrderTotLoadV4Request.Request.InFieldValueCollection.InFieldValues
                .FirstOrDefault(o => o.Fieldname.Equals("AuthNumber"))
                .Fieldvalue
        );
    }

    [TestCase("mastercard", "MASTERCARD")]
    [TestCase("AmericanExpress", "AMEX")]
    [TestCase("DISCOVER", "DISCOVER")]
    [TestCase("Visa", "VISA")]
    [TestCase("other", "other")]
    public void TransformCardTypeToPayType_Should_Transform_Credit_Card_Type(
        string cardType,
        string transformedCardType
    )
    {
        var result = (string)this.RunPrivateMethod("TransformCardTypeToPayType", cardType);

        Assert.AreEqual(transformedCardType, result);
    }

    protected override SFOEOrderTotLoadV4Parameter GetDefaultParameter()
    {
        return new SFOEOrderTotLoadV4Parameter { CustomerOrder = Some.CustomerOrder() };
    }

    protected override SFOEOrderTotLoadV4Result GetDefaultResult()
    {
        return new SFOEOrderTotLoadV4Result
        {
            SFOEOrderTotLoadV4Request = new SFOEOrderTotLoadV4Request { Request = new Request() }
        };
    }

    private void WhenMerchantIdIs(int merchantId)
    {
        this.cenposSettings.Setup(o => o.MerchantId).Returns(merchantId);
    }
}
