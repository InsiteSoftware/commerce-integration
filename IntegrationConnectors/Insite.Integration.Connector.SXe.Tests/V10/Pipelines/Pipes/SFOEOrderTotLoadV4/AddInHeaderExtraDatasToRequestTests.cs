namespace Insite.Integration.Connector.SXe.Tests.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Linq;

using Moq;
using NUnit.Framework;

using Insite.Core.Plugins.EntityUtilities;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;

[TestFixture]
public class AddInHeaderExtraDatasToRequestTests
    : BaseForPipeTests<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    private Mock<ICustomerOrderUtilities> customerOrderUtilities;

    public override Type PipeType => typeof(AddInHeaderExtraDatasToRequest);

    public override void SetUp()
    {
        this.customerOrderUtilities = this.container.GetMock<ICustomerOrderUtilities>();
    }

    [Test]
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_A_InHeaderExtraData2_For_ShippingCharges()
    {
        var customerOrder = Some.CustomerOrder().WithShippingCharges(14).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        var expectedFieldValue =
            $"addonno=2\taddonamt={customerOrder.ShippingCharges}\taddontype=$";

        Assert.AreEqual(
            expectedFieldValue,
            result.SFOEOrderTotLoadV4Request.Inheaderextradata
                .FirstOrDefault(o => o.FieldName.Equals("addon"))
                .FieldValue
        );
    }

    [Test]
    public void Execute_Should_Populate_A_InHeaderExtraData2_For_HandlingCharges()
    {
        var customerOrder = Some.CustomerOrder().WithHandlingCharges(3).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        var expectedFieldValue =
            $"addonno=3\taddonamt={customerOrder.HandlingCharges}\taddontype=$";

        Assert.AreEqual(
            expectedFieldValue,
            result.SFOEOrderTotLoadV4Request.Inheaderextradata
                .FirstOrDefault(o => o.FieldName.Equals("addon"))
                .FieldValue
        );
    }

    [Test]
    public void Execute_Should_Populate_A_InHeaderExtraData2_For_OtherCharges()
    {
        var customerOrder = Some.CustomerOrder().WithOtherCharges(12).Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        var expectedFieldValue = $"addonno=10\taddonamt={customerOrder.OtherCharges}\taddontype=$";

        Assert.AreEqual(
            expectedFieldValue,
            result.SFOEOrderTotLoadV4Request.Inheaderextradata
                .FirstOrDefault(o => o.FieldName.Equals("addon"))
                .FieldValue
        );
    }

    [Test]
    public void Execute_Should_Populate_TermsType()
    {
        var customerOrder = Some.CustomerOrder().WithTermsCode("ABC").Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.TermsCode,
            result.SFOEOrderTotLoadV4Request.Inheaderextradata
                .FirstOrDefault(o => o.FieldName.Equals("termstype"))
                .FieldValue
        );
    }

    [Test]
    public void Execute_Should_Populate_WebOrderNumber()
    {
        var customerOrder = Some.CustomerOrder().WithOrderNumber("ABC").Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderNumber,
            result.SFOEOrderTotLoadV4Request.Inheaderextradata
                .FirstOrDefault(o => o.FieldName.Equals("iondata"))
                .FieldValue
        );
    }

    [Test]
    public void Execute_Should_Populate_Discount_Amount_When_Discount_Amount_Is_Greater_Than_Zero()
    {
        var customerOrder = Some.CustomerOrder().Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenGetPromotionOrderDiscountTotalIs(customerOrder, 10);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "10",
            result.SFOEOrderTotLoadV4Request.Inheaderextradata
                .FirstOrDefault(o => o.FieldName.Equals("discountamt"))
                .FieldValue
        );
    }

    [Test]
    public void Execute_Should_Not_Populate_Discount_Amount_When_Discount_Amount_Is_Less_Than_Or_Equal_To_Zero()
    {
        var customerOrder = Some.CustomerOrder().Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenGetPromotionOrderDiscountTotalIs(customerOrder, 0);

        var result = this.RunExecute(parameter);

        Assert.IsFalse(
            result.SFOEOrderTotLoadV4Request.Inheaderextradata.Any(
                o => o.FieldName.Equals("discountamt")
            )
        );
    }

    [Test]
    public void Execute_Should_Populate_DoNotRecalculatePrice_When_Any_Order_Lines_Have_A_Promotion()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.OrderLine())
            .With(Some.OrderLine().With(Some.CustomerOrderPromotion()))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.SFOEOrderTotLoadV4Request.Inheaderextradata.Any(
                o => o.FieldName.Equals("donotrecalculateprice")
            )
        );
    }

    [Test]
    public void Execute_Should_Not_Populate_DoNotRecalculatePrice_When_No_Order_Lines_Have_A_Promotion()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.OrderLine())
            .With(Some.OrderLine())
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.IsFalse(
            result.SFOEOrderTotLoadV4Request.Inheaderextradata.Any(
                o => o.FieldName.Equals("donotrecalculateprice")
            )
        );
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

    private void WhenGetPromotionOrderDiscountTotalIs(
        CustomerOrder customerOrder,
        decimal promotionOrderDiscountTotal
    )
    {
        this.customerOrderUtilities
            .Setup(o => o.GetPromotionOrderDiscountTotal(customerOrder))
            .Returns(promotionOrderDiscountTotal);
    }
}
