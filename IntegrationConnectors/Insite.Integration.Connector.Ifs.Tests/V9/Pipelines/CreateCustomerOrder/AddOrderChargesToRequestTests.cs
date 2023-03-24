namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.CreateCustomerOrder;

using System;
using System.Linq;

using Moq;
using NUnit.Framework;

using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;

[TestFixture]
public class AddOrderChargesToRequestTests
    : BaseForPipeTests<CreateCustomerOrderParameter, CreateCustomerOrderResult>
{
    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    public override Type PipeType => typeof(AddOrderChargesToRequest);

    public override void SetUp()
    {
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();

        this.WhenIfsChargeTypeFreightIs(string.Empty);
        this.WhenIfsChargeTypePromotionIs(string.Empty);
    }

    [Test]
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_OrderCharges_With_CustomerOrderPromotions()
    {
        var customerOrder = Some.CustomerOrder().With(Some.CustomerOrderPromotion().WithAmount(12));

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenIfsChargeTypePromotionIs("PROMOTION");

        var result = this.RunExecute(parameter);

        Assert.AreEqual("PROMOTION", result.CustomerOrder.charges.First().chargeType);
        Assert.AreEqual(-12, result.CustomerOrder.charges.First().chargeAmount);
        Assert.That(result.CustomerOrder.charges.First().lineNo, Is.Null.Or.Empty);
        Assert.AreEqual(1, result.CustomerOrder.charges.First().chargedQty);
    }

    [Test]
    public void Execute_Should_Populate_OrderCharges_With_OrderLine_CustomerOrderPromotions()
    {
        var orderLineId = Guid.NewGuid();
        var customerOrder = Some.CustomerOrder()
            .With(Some.OrderLine().WithId(orderLineId).WithQtyOrdered(2).WithLine(1))
            .With(Some.CustomerOrderPromotion().WithOrderLineId(orderLineId).WithAmount(12))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenIfsChargeTypePromotionIs("PROMOTION");

        var result = this.RunExecute(parameter);

        Assert.AreEqual("PROMOTION", result.CustomerOrder.charges.First().chargeType);
        Assert.AreEqual(-6, result.CustomerOrder.charges.First().chargeAmount);
        Assert.AreEqual("1", result.CustomerOrder.charges.First().lineNo);
        Assert.AreEqual(2, result.CustomerOrder.charges.First().chargedQty);
    }

    [Test]
    public void Execute_Should_Populate_OrderCharges_With_Shipping_And_Handling()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShippingCharges(10)
            .WithHandlingCharges(12)
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenIfsChargeTypeFreightIs("FREIGHT");

        var result = this.RunExecute(parameter);

        Assert.AreEqual("FREIGHT", result.CustomerOrder.charges.First().chargeType);
        Assert.AreEqual(22, result.CustomerOrder.charges.First().chargeAmount);
        Assert.That(result.CustomerOrder.charges.First().lineNo, Is.Null.Or.Empty);
        Assert.AreEqual(1, result.CustomerOrder.charges.First().chargedQty);
    }

    [Test]
    public void Execute_Should_Set_Order_Charges_If_CustomerOrder_Has_No_Promotions_And_No_Shipping_And_Handling_Charges()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = Some.CustomerOrder();

        var result = this.RunExecute(parameter);

        Assert.IsNull(result.CustomerOrder.charges);
    }

    protected override CreateCustomerOrderResult GetDefaultResult()
    {
        return new CreateCustomerOrderResult { CustomerOrder = new customerOrder() };
    }

    protected void WhenIfsChargeTypePromotionIs(string ifsChargeTypePromotion)
    {
        this.integrationConnectorSettings
            .Setup(o => o.IfsChargeTypePromotion)
            .Returns(ifsChargeTypePromotion);
    }

    protected void WhenIfsChargeTypeFreightIs(string ifsChargeTypeFreight)
    {
        this.integrationConnectorSettings
            .Setup(o => o.IfsChargeTypeFreight)
            .Returns(ifsChargeTypeFreight);
    }
}
