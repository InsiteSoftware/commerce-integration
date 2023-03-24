namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.OEPricingMultipleV4;

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.OEPricingMultipleV4;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4;

[TestFixture]
public class GetInventoryFromResponseTests
    : BaseForPipeTests<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    private IList<Product> products;

    private IList<Warehouse> warehouses;

    public override Type PipeType => typeof(GetInventoryFromResponse);

    public override void SetUp()
    {
        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);

        this.warehouses = new List<Warehouse>();
        this.fakeUnitOfWork.SetupEntityList(this.warehouses);
    }

    [Test]
    public void Order_Is_700()
    {
        Assert.AreEqual(700, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Create_Get_Inventory_Results_For_Pricing_Service_Parameters()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var pricingServiceParameter = new PricingServiceParameter(Guid.Empty) { Product = product };

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(null, pricingServiceParameter);

        var outPrice = this.CreatePriceOutV2("123", string.Empty, string.Empty, 25);

        var result = this.CreateGetOEPricingMultipleV4Result(outPrice);

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(result.PricingServiceResults);
        Assert.AreEqual(
            25,
            result.PricingServiceResults
                .First()
                .Value.GetInventoryResult.Inventories.First()
                .Value.QtyOnHand
        );
    }

    [Test]
    public void Execute_Should_Create_Get_Inventory_Result_For_Get_Inventory_Parameter_Products()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var getInventoryParameter = new GetInventoryParameter
        {
            Products = new List<Product> { product }
        };

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(getInventoryParameter);

        var outPrice = this.CreatePriceOutV2("123", string.Empty, string.Empty, 25);

        var result = this.CreateGetOEPricingMultipleV4Result(outPrice);

        result = this.RunExecute(parameter, result);

        Assert.AreEqual(25, result.GetInventoryResult.Inventories.First().Value.QtyOnHand);
    }

    [Test]
    public void Execute_Should_Create_Get_Inventory_Result_For_Get_Inventory_Parameter_ProductIds()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var getInventoryParameter = new GetInventoryParameter
        {
            ProductIds = new List<Guid> { product.Id }
        };

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(getInventoryParameter);

        var outPrice = this.CreatePriceOutV2("123", string.Empty, string.Empty, 25);

        var result = this.CreateGetOEPricingMultipleV4Result(outPrice);

        this.WhenExists(product);

        result = this.RunExecute(parameter, result);

        Assert.AreEqual(25M, result.GetInventoryResult.Inventories.First().Value.QtyOnHand);
    }

    [Test]
    public void Execute_Should_Populate_Warehouse_Qty_On_Hand_Dtos()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var getInventoryParameter = new GetInventoryParameter
        {
            Products = new List<Product> { product }
        };

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(getInventoryParameter);

        var outPriceLocationQuantityOne = this.CreatePriceOutV2(
            "123",
            string.Empty,
            "Warehouse One",
            10
        );
        var outPriceLocationQuantityTwo = this.CreatePriceOutV2(
            "123",
            string.Empty,
            "Warehouse Two",
            15
        );

        var outPrice = this.CreatePriceOutV2("123", string.Empty, string.Empty, 25);

        var result = this.CreateGetOEPricingMultipleV4Result(
            outPrice,
            outPriceLocationQuantityOne,
            outPriceLocationQuantityTwo
        );

        result = this.RunExecute(parameter, result);

        CollectionAssert.IsNotEmpty(
            result.GetInventoryResult.Inventories.Values.First().WarehouseQtyOnHandDtos
        );

        Assert.IsTrue(
            result.GetInventoryResult.Inventories.Values
                .First()
                .WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(outPriceLocationQuantityOne.Whse)
                        && o.QtyOnHand.Equals(outPriceLocationQuantityOne.Netavail)
                )
        );

        Assert.IsTrue(
            result.GetInventoryResult.Inventories.Values
                .First()
                .WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(outPriceLocationQuantityTwo.Whse)
                        && o.QtyOnHand.Equals(outPriceLocationQuantityTwo.Netavail)
                )
        );
    }

    protected OEPricingMultipleV4Parameter CreateGetOEPricingMultipleV4Parameter(
        GetInventoryParameter getInventoryParameter,
        params PricingServiceParameter[] pricingServiceParameters
    )
    {
        return new OEPricingMultipleV4Parameter
        {
            GetInventoryParameter = getInventoryParameter,
            PricingServiceParameters = pricingServiceParameters.ToList()
        };
    }

    protected PriceOutV2 CreatePriceOutV2(
        string productCode,
        string unitOfMeasue,
        string warehouse,
        decimal netAvailable
    )
    {
        return new PriceOutV2
        {
            Prod = productCode,
            Unit = unitOfMeasue,
            Whse = warehouse,
            Netavail = netAvailable
        };
    }

    protected OEPricingMultipleV4Result CreateGetOEPricingMultipleV4Result(
        params PriceOutV2[] priceOutV2s
    )
    {
        return new OEPricingMultipleV4Result
        {
            OEPricingMultipleV4Response = new OEPricingMultipleV4Response
            {
                Response = new Response
                {
                    PriceOutV2Collection = new PriceOutV2Collection
                    {
                        PriceOutV2s = priceOutV2s.ToList()
                    }
                }
            }
        };
    }

    protected void WhenExists(Product product)
    {
        this.products.Add(product);
    }

    protected void WhenExists(Warehouse warehouse)
    {
        this.warehouses.Add(warehouse);
    }
}
