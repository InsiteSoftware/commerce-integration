namespace Insite.Integration.Connector.SXe.Tests.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Linq;

using Moq;
using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;
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
            result.SFOEOrderTotLoadV4Request.Ininfieldvalue == null
                || !result.SFOEOrderTotLoadV4Request.Ininfieldvalue.Any()
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
            result.SFOEOrderTotLoadV4Request.Ininfieldvalue
                .FirstOrDefault(o => o.FieldName.Equals("MerchantID"))
                .FieldValue
        );
        Assert.AreEqual(
            "cenpos",
            result.SFOEOrderTotLoadV4Request.Ininfieldvalue
                .FirstOrDefault(o => o.FieldName.Equals("PaymentType"))
                .FieldValue
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().CreditCardNumber,
            result.SFOEOrderTotLoadV4Request.Ininfieldvalue
                .FirstOrDefault(o => o.FieldName.Equals("CardNumber"))
                .FieldValue
        );
        Assert.AreEqual(
            "VISA",
            result.SFOEOrderTotLoadV4Request.Ininfieldvalue
                .FirstOrDefault(o => o.FieldName.Equals("ProcPaymentType"))
                .FieldValue
        );
        Assert.AreEqual(
            null,
            result.SFOEOrderTotLoadV4Request.Ininfieldvalue
                .FirstOrDefault(o => o.FieldName.Equals("Token"))
                .FieldValue
        );
        Assert.AreEqual(
            "12",
            result.SFOEOrderTotLoadV4Request.Ininfieldvalue
                .FirstOrDefault(o => o.FieldName.Equals("AuthAmt"))
                .FieldValue
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().PNRef,
            result.SFOEOrderTotLoadV4Request.Ininfieldvalue
                .FirstOrDefault(o => o.FieldName.Equals("ReferenceNumber"))
                .FieldValue
        );
        Assert.AreEqual(
            customerOrder.CreditCardTransactions.First().AuthCode,
            result.SFOEOrderTotLoadV4Request.Ininfieldvalue
                .FirstOrDefault(o => o.FieldName.Equals("AuthNumber"))
                .FieldValue
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
            SFOEOrderTotLoadV4Request = new SFOEOrderTotLoadV4Request()
        };
    }

    private void WhenMerchantIdIs(int merchantId)
    {
        this.cenposSettings.Setup(o => o.MerchantId).Returns(merchantId);
    }
}
