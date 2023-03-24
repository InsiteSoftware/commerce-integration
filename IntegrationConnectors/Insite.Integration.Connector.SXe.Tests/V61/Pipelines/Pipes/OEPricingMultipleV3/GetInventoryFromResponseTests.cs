namespace Insite.Integration.Connector.SXe.Tests.V6.Pipelines.Pipes.OEPricingMultipleV3;

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.OEPricingMultipleV3;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;

[TestFixture]
public class GetInventoryFromResponseTests
    : BaseForPipeTests<OEPricingMultipleV3Parameter, OEPricingMultipleV3Result>
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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(null, pricingServiceParameter);

        var outPrice = this.CreateOutPrice("123", string.Empty, string.Empty, 25);

        var result = this.CreateOEPricingMultipleV3Result(outPrice);

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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(getInventoryParameter);

        var outPrice = this.CreateOutPrice("123", string.Empty, string.Empty, 25);

        var result = this.CreateOEPricingMultipleV3Result(outPrice);

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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(getInventoryParameter);

        var outPrice = this.CreateOutPrice("123", string.Empty, string.Empty, 25);

        var result = this.CreateOEPricingMultipleV3Result(outPrice);

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

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(getInventoryParameter);

        var outPriceLocationQuantityOne = this.CreateOutPrice(
            "123",
            string.Empty,
            "Warehouse One",
            10
        );
        var outPriceLocationQuantityTwo = this.CreateOutPrice(
            "123",
            string.Empty,
            "Warehouse Two",
            15
        );

        var outPrice = this.CreateOutPrice("123", string.Empty, string.Empty, 25);

        var result = this.CreateOEPricingMultipleV3Result(
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
                        o.Name.Equals(outPriceLocationQuantityOne.warehouse)
                        && o.QtyOnHand.Equals(outPriceLocationQuantityOne.netAvailable)
                )
        );

        Assert.IsTrue(
            result.GetInventoryResult.Inventories.Values
                .First()
                .WarehouseQtyOnHandDtos.Any(
                    o =>
                        o.Name.Equals(outPriceLocationQuantityTwo.warehouse)
                        && o.QtyOnHand.Equals(outPriceLocationQuantityTwo.netAvailable)
                )
        );
    }

    protected OEPricingMultipleV3Parameter CreateGetOEPricingMultipleV3Parameter(
        GetInventoryParameter getInventoryParameter,
        params PricingServiceParameter[] pricingServiceParameters
    )
    {
        return new OEPricingMultipleV3Parameter
        {
            GetInventoryParameter = getInventoryParameter,
            PricingServiceParameters = pricingServiceParameters.ToList()
        };
    }

    protected OEPricingMultipleV3outputPrice CreateOutPrice(
        string productCode,
        string unitOfMeasue,
        string warehouse,
        decimal netAvailable
    )
    {
        return new OEPricingMultipleV3outputPrice
        {
            productCode = productCode,
            unitOfMeasure = unitOfMeasue,
            warehouse = warehouse,
            netAvailable = netAvailable
        };
    }

    protected OEPricingMultipleV3Result CreateOEPricingMultipleV3Result(
        params OEPricingMultipleV3outputPrice[] outPrices
    )
    {
        return new OEPricingMultipleV3Result
        {
            OEPricingMultipleV3Response = new OEPricingMultipleV3Response
            {
                arrayPrice = outPrices.ToList()
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
