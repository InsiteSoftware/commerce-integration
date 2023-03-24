namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.LineItemPriceAndAvailability;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;

[TestFixture]
public class AddProductsToRequestTests
    : BaseForPipeTests<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    private IList<Product> products;

    public override Type PipeType => typeof(AddProductsToRequest);

    public override void SetUp()
    {
        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_LineItemPriceAndAvailabilityRequestItem_Collection_With_Products()
    {
        var pricingServiceParameter1 = new PricingServiceParameter(Guid.Empty)
        {
            Product = Some.Product().WithErpNumber("1"),
            UnitOfMeasure = "EA",
            QtyOrdered = 1,
            Warehouse = "W1"
        };

        var pricingServiceParameter2 = new PricingServiceParameter(Guid.Empty)
        {
            Product = Some.Product().WithErpNumber("2"),
            UnitOfMeasure = "CS",
            QtyOrdered = 2,
            Warehouse = "W2"
        };

        var parameter = new LineItemPriceAndAvailabilityParameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>
            {
                pricingServiceParameter1,
                pricingServiceParameter2
            }
        };

        var result = this.RunExecute(parameter);

        this.VerifyLineItemPriceAndAvailabilityRequestItemCreatedFromPricingServiceParameter(
            result.LineItemPriceAndAvailabilityRequest,
            pricingServiceParameter1,
            pricingServiceParameter1.Product
        );
        this.VerifyLineItemPriceAndAvailabilityRequestItemCreatedFromPricingServiceParameter(
            result.LineItemPriceAndAvailabilityRequest,
            pricingServiceParameter2,
            pricingServiceParameter2.Product
        );
    }

    [Test]
    public void Execute_Should_Get_LineItemPriceAndAvailabilityRequestItem_Collection_With_ProductIds()
    {
        var product1 = Some.Product().WithErpNumber("1").Build();
        var product2 = Some.Product().WithErpNumber("2").Build();

        this.WhenExists(product1);
        this.WhenExists(product2);

        var pricingServiceParameter1 = new PricingServiceParameter(Guid.Empty)
        {
            ProductId = product1.Id,
            UnitOfMeasure = "EA",
            QtyOrdered = 1,
            Warehouse = "W1"
        };

        var pricingServiceParameter2 = new PricingServiceParameter(Guid.Empty)
        {
            ProductId = product2.Id,
            UnitOfMeasure = "CS",
            QtyOrdered = 2,
            Warehouse = "W2"
        };

        var parameter = new LineItemPriceAndAvailabilityParameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>
            {
                pricingServiceParameter1,
                pricingServiceParameter2
            }
        };

        var result = this.RunExecute(parameter);

        this.VerifyLineItemPriceAndAvailabilityRequestItemCreatedFromPricingServiceParameter(
            result.LineItemPriceAndAvailabilityRequest,
            pricingServiceParameter1,
            product1
        );
        this.VerifyLineItemPriceAndAvailabilityRequestItemCreatedFromPricingServiceParameter(
            result.LineItemPriceAndAvailabilityRequest,
            pricingServiceParameter2,
            product2
        );
    }

    protected override LineItemPriceAndAvailabilityResult GetDefaultResult()
    {
        return new LineItemPriceAndAvailabilityResult
        {
            LineItemPriceAndAvailabilityRequest = new LineItemPriceAndAvailabilityRequest()
        };
    }

    protected void WhenExists(Product product)
    {
        this.products.Add(product);
    }

    protected void VerifyLineItemPriceAndAvailabilityRequestItemCreatedFromPricingServiceParameter(
        LineItemPriceAndAvailabilityRequest lineItemPriceAndAvailabilityRequest,
        PricingServiceParameter pricingServiceParameter,
        Product product
    )
    {
        Assert.IsTrue(
            lineItemPriceAndAvailabilityRequest.Items.Any(
                o =>
                    o.ItemNumber == product.ErpNumber
                    && o.UnitofMeasure == pricingServiceParameter.UnitOfMeasure
                    && o.OrderQuantity == pricingServiceParameter.QtyOrdered.ToString()
                    && o.WarehouseID == pricingServiceParameter.Warehouse
            )
        );
    }
}
