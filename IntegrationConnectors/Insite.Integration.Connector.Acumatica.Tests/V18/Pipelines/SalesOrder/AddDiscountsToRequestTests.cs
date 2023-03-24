namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.SalesOrder;

using System;
using System.Linq;
using Insite.Core.Plugins.EntityUtilities;
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
public class AddDiscountsToRequestTests : BaseForPipeTests<SalesOrderParameter, SalesOrderResult>
{
    private Mock<ICustomerOrderUtilities> customerOrderUtilities;

    public override Type PipeType => typeof(AddDiscountsToRequest);

    public override void SetUp()
    {
        this.customerOrderUtilities = this.container.GetMock<ICustomerOrderUtilities>();
    }

    [Test]
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_DiscountDetails_From_CustomerOrderPromotions()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.CustomerOrderPromotion()
                    .With(
                        Some.Promotion().WithDescription("Promo Description").WithName("Promo Name")
                    )
                    .With(Some.PromotionResult().With(Some.Product().WithErpNumber("Erp123")))
                    .WithAmount(12)
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenGetOrderSubTotalWithOutProductDiscountsIs(customerOrder, 0);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(1, result.SalesOrderRequest.DiscountDetails.First().rowNumber);
        Assert.AreEqual(
            customerOrder.CustomerOrderPromotions.First().Promotion.Description,
            result.SalesOrderRequest.DiscountDetails.First().Description
        );
        Assert.AreEqual(
            customerOrder.CustomerOrderPromotions.First().Promotion.Name,
            result.SalesOrderRequest.DiscountDetails.First().DiscountCode
        );
        Assert.AreEqual(
            customerOrder.CustomerOrderPromotions.First().PromotionResult.Product.ErpNumber,
            result.SalesOrderRequest.DiscountDetails.First().FreeItem
        );
        Assert.AreEqual(
            customerOrder.CustomerOrderPromotions.First().Amount,
            result.SalesOrderRequest.DiscountDetails.First().DiscountAmount
        );
    }

    [Test]
    public void GetDiscountableAmount_Should_Get_Amount_From_CustomerOrderUtilities_When_CustomerOrderPromotion_OrderLine_Is_Null()
    {
        var customerOrder = Some.CustomerOrder().Build();
        var customerOrderPromotion = Some.CustomerOrderPromotion().Build();

        this.WhenGetOrderSubTotalWithOutProductDiscountsIs(customerOrder, 12);

        var result = (decimal)
            this.RunPrivateMethod("GetDiscountableAmount", customerOrder, customerOrderPromotion);

        Assert.AreEqual(12, result);
    }

    [Test]
    public void GetDiscountableAmount_Should_Get_Amount_From_OrderLine_UnitNetPrice_Plus_CustomerOrderPromotion_Amount()
    {
        var customerOrder = Some.CustomerOrder().Build();
        var customerOrderPromotion = Some.CustomerOrderPromotion()
            .With(Some.OrderLine().WithUnitNetPrice(20))
            .WithAmount(5)
            .Build();

        this.WhenGetOrderSubTotalWithOutProductDiscountsIs(customerOrder, 12);

        var result = (decimal)
            this.RunPrivateMethod("GetDiscountableAmount", customerOrder, customerOrderPromotion);

        Assert.AreEqual(25, result);
    }

    [Test]
    public void GetDiscountPercent_Should_Return_Zero_When_CustomerOrderPromotion_PromotionResult_IsNotPercent()
    {
        var customerOrderPromotion = Some.CustomerOrderPromotion()
            .With(Some.PromotionResult().WithIsPercent(false))
            .Build();

        var result = (decimal)this.RunPrivateMethod("GetDiscountPercent", customerOrderPromotion);

        Assert.AreEqual(0, result);
    }

    [Test]
    public void GetDiscountPercent_Should_Return_CustomerOrderPromotion_Amount_When_CustomerOrderPromotion_PromotionResult_IsPercent()
    {
        var customerOrderPromotion = Some.CustomerOrderPromotion()
            .With(Some.PromotionResult().WithIsPercent(true))
            .WithAmount(10)
            .Build();

        var result = (decimal)this.RunPrivateMethod("GetDiscountPercent", customerOrderPromotion);

        Assert.AreEqual(10, result);
    }

    [Test]
    public void GetFreeItemQty_Should_Return_Zero_When_PromotionResult_Product_Is_Null()
    {
        var customerOrderPromotion = Some.CustomerOrderPromotion()
            .With(Some.PromotionResult())
            .Build();

        var result = (decimal)this.RunPrivateMethod("GetFreeItemQty", customerOrderPromotion);

        Assert.AreEqual(0, result);
    }

    [Test]
    public void GetFreeItemQty_Should_Return_CustomerOrderPromotion_PromotionResult_Amount_When_PromotionResult_Product_Is_Not_Null()
    {
        var customerOrderPromotion = Some.CustomerOrderPromotion()
            .With(Some.PromotionResult().With(Some.Product()).WithAmount(1))
            .Build();

        var result = (decimal)this.RunPrivateMethod("GetFreeItemQty", customerOrderPromotion);

        Assert.AreEqual(1, result);
    }

    protected override SalesOrderResult GetDefaultResult()
    {
        return new SalesOrderResult { SalesOrderRequest = new SalesOrder() };
    }

    private void WhenGetOrderSubTotalWithOutProductDiscountsIs(
        CustomerOrder customerOrder,
        decimal amount
    )
    {
        this.customerOrderUtilities
            .Setup(o => o.GetOrderSubTotalWithOutProductDiscounts(customerOrder))
            .Returns(amount);
    }
}
