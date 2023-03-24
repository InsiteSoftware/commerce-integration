namespace Insite.Integration.Connector.CloudSuiteDistribution.Tests.V11.Pipelines.GetPricingAndInventoryStock;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Entities.Dtos;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.OePricingMultipleV4;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetPricingAndInventoryStock;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class GetInventoryFromResponseTests
    : BaseForPipeTests<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
{
    private Mock<IProductHelper> productHelper;

    private Mock<IWarehouseHelper> warehouseHelper;

    private IList<Product> products;

    public override Type PipeType => typeof(GetInventoryFromResponse);

    public override void SetUp()
    {
        this.productHelper = this.container.GetMock<IProductHelper>();
        this.warehouseHelper = this.container.GetMock<IWarehouseHelper>();

        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_700()
    {
        this.pipe.Order.Should().Be(700);
    }

    [Test]
    public void Execute_Should_Get_GetInventoryResult_For_PricingServiceParameter_Product()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.Empty) { Product = product }
        };

        var parameter = new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };

        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = CreateOePricingMultipleV4Response(
                (product.ErpNumber, string.Empty, string.Empty, 100)
            )
        };

        result = this.RunExecute(parameter, result);

        result.PricingServiceResults
            .First()
            .Value.GetInventoryResult.Inventories.First()
            .Value.QtyOnHand.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s
                    .First()
                    .NetAvail
            );
        result.GetInventoryResult.Inventories
            .First()
            .Value.QtyOnHand.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s
                    .First()
                    .NetAvail
            );
    }

    [Test]
    public void Execute_Should_Get_GetInventoryResult_For_PricingServiceParameter_Product_And_UnitOfMeasure()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.Empty) { Product = product, UnitOfMeasure = "EA" }
        };

        var parameter = new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };

        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = CreateOePricingMultipleV4Response(
                (product.ErpNumber, string.Empty, string.Empty, 100),
                (product.ErpNumber, "EA", string.Empty, 200)
            )
        };

        result = this.RunExecute(parameter, result);

        result.PricingServiceResults
            .First()
            .Value.GetInventoryResult.Inventories.First()
            .Value.QtyOnHand.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s[
                    1
                ].NetAvail
            );
        result.GetInventoryResult.Inventories
            .First()
            .Value.QtyOnHand.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s[
                    1
                ].NetAvail
            );
    }

    [Test]
    public void Execute_Should_Get_GetInventoryResult_For_PricingServiceParameter_Product_And_Warehouse_Returned_From_WarehouseHelper_GetWarehouse()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var warehouse = new WarehouseDto(Some.Warehouse().WithName("MAIN"));
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.Empty) { Product = product, UnitOfMeasure = "EA" }
        };

        var parameter = new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };

        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = CreateOePricingMultipleV4Response(
                (product.ErpNumber, string.Empty, string.Empty, 100),
                (product.ErpNumber, string.Empty, warehouse.Name, 200)
            )
        };

        this.WhenGetWarehouseIs(pricingServiceParameters.First(), warehouse);

        result = this.RunExecute(parameter, result);

        result.PricingServiceResults
            .First()
            .Value.GetInventoryResult.Inventories.First()
            .Value.QtyOnHand.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s[
                    1
                ].NetAvail
            );
        result.GetInventoryResult.Inventories
            .First()
            .Value.QtyOnHand.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s[
                    1
                ].NetAvail
            );
    }

    [Test]
    public void Execute_Should_Get_GetInventoryResult_For_PricingServiceParameter_ProductId()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(product.Id)
        };

        this.WhenExists(product);

        var parameter = new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };

        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = CreateOePricingMultipleV4Response(
                (product.ErpNumber, string.Empty, string.Empty, 100)
            )
        };

        result = this.RunExecute(parameter, result);

        result.PricingServiceResults
            .First()
            .Value.GetInventoryResult.Inventories.First()
            .Value.QtyOnHand.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s
                    .First()
                    .NetAvail
            );
        result.GetInventoryResult.Inventories
            .First()
            .Value.QtyOnHand.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s
                    .First()
                    .NetAvail
            );
    }

    [Test]
    public void Execute_Should_Get_GetInventoryResult_For_Products_Returned_From_ProductHelper_GetProducts()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();

        var parameter = new GetPricingAndInventoryStockParameter
        {
            GetInventoryParameter = new GetInventoryParameter()
        };

        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = CreateOePricingMultipleV4Response(
                (product.ErpNumber, string.Empty, string.Empty, 100)
            )
        };

        this.WhenGetProductsIs(parameter.GetInventoryParameter, product);

        result = this.RunExecute(parameter, result);

        result.GetInventoryResult.Inventories
            .First()
            .Value.QtyOnHand.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s
                    .First()
                    .NetAvail
            );
    }

    [Test]
    public void Execute_Should_Get_GetInventoryResult_For_Products_And_Their_UnitOfMeasure_Returned_From_ProductHelper_GetProducts()
    {
        var product = Some.Product().WithErpNumber("ABC123").WithUnitOfMeasure("EA").Build();

        var parameter = new GetPricingAndInventoryStockParameter
        {
            GetInventoryParameter = new GetInventoryParameter()
        };

        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = CreateOePricingMultipleV4Response(
                (product.ErpNumber, string.Empty, string.Empty, 100),
                (product.ErpNumber, product.UnitOfMeasure, string.Empty, 200)
            )
        };

        this.WhenGetProductsIs(parameter.GetInventoryParameter, product);

        result = this.RunExecute(parameter, result);

        result.GetInventoryResult.Inventories
            .First()
            .Value.QtyOnHand.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s[
                    1
                ].NetAvail
            );
    }

    [Test]
    public void Execute_Should_Get_GetInventoryResult_For_Products_Returned_From_ProductHelper_GetProducts_And_Warehouse_Returned_From_WarehouseHelper_GetWarehouse()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var warehouse = new WarehouseDto(Some.Warehouse().WithName("MAIN"));

        var parameter = new GetPricingAndInventoryStockParameter
        {
            GetInventoryParameter = new GetInventoryParameter()
        };

        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = CreateOePricingMultipleV4Response(
                (product.ErpNumber, string.Empty, string.Empty, 100),
                (product.ErpNumber, string.Empty, warehouse.Name, 200)
            )
        };

        this.WhenGetProductsIs(parameter.GetInventoryParameter, product);
        this.WhenGetWarehouseIs(parameter.GetInventoryParameter, warehouse);

        result = this.RunExecute(parameter, result);

        result.GetInventoryResult.Inventories
            .First()
            .Value.QtyOnHand.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s[
                    1
                ].NetAvail
            );
    }

    [Test]
    public void Execute_Should_Return_QtyOnHand_Zero_When_No_Results()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = new GetPricingAndInventoryStockParameter
        {
            GetInventoryParameter = new GetInventoryParameter()
        };

        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = CreateOePricingMultipleV4Response()
        };

        this.WhenGetProductsIs(parameter.GetInventoryParameter, product);

        result = this.RunExecute(parameter, result);

        result.GetInventoryResult.Inventories.Should().NotBeEmpty();
        result.GetInventoryResult.Inventories.Should().ContainKey(product.Id);
        result.GetInventoryResult.Inventories.First().Value.QtyOnHand.Should().Be(0);
    }

    [Test]
    public void Execute_Should_Return_QtyOnHand_Zero_When_No_Results_For_Product()
    {
        var product = Some.Product().WithErpNumber("123").Build();

        var parameter = new GetPricingAndInventoryStockParameter
        {
            GetInventoryParameter = new GetInventoryParameter()
        };

        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = CreateOePricingMultipleV4Response(
                (string.Empty, string.Empty, string.Empty, 100)
            )
        };

        this.WhenGetProductsIs(parameter.GetInventoryParameter, product);

        result = this.RunExecute(parameter, result);

        result.GetInventoryResult.Inventories.Should().NotBeEmpty();
        result.GetInventoryResult.Inventories.Should().ContainKey(product.Id);
        result.GetInventoryResult.Inventories.First().Value.QtyOnHand.Should().Be(0);
    }

    [Test]
    public void Execute_Should_Calculate_QtyOnHand_As_NetAvail_Multiplied_By_UnitConv()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.Empty) { Product = product }
        };

        var parameter = new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };

        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = new OePricingMultipleV4Response
            {
                Response = new Response
                {
                    PriceOutV2Collection = new PriceOutV2Collection
                    {
                        PriceOutV2s = new List<PriceOutV2>
                        {
                            new PriceOutV2
                            {
                                Prod = product.ErpNumber,
                                NetAvail = 10,
                                UnitConv = 2
                            }
                        }
                    }
                }
            }
        };

        result = this.RunExecute(parameter, result);

        result.GetInventoryResult.Inventories
            .First()
            .Value.QtyOnHand.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s
                    .First()
                    .NetAvail
                    * result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s
                        .First()
                        .UnitConv
            );
    }

    [Test]
    public void Execute_Should_Populate_WarehouseQtyOnHandDtos()
    {
        var product = Some.Product().WithErpNumber("ABC123").Build();
        var pricingServiceParameters = new List<PricingServiceParameter>
        {
            new PricingServiceParameter(Guid.Empty) { Product = product }
        };

        var parameter = new GetPricingAndInventoryStockParameter
        {
            PricingServiceParameters = pricingServiceParameters
        };

        var result = new GetPricingAndInventoryStockResult
        {
            OePricingMultipleV4Response = CreateOePricingMultipleV4Response(
                (product.ErpNumber, string.Empty, "1", 100),
                (product.ErpNumber, string.Empty, "2", 200)
            )
        };

        result = this.RunExecute(parameter, result);

        result.PricingServiceResults
            .First()
            .Value.GetInventoryResult.Inventories.First()
            .Value.WarehouseQtyOnHandDtos[0].Name
            .Should()
            .Be("1");
        result.PricingServiceResults
            .First()
            .Value.GetInventoryResult.Inventories.First()
            .Value.WarehouseQtyOnHandDtos[0].QtyOnHand
            .Should()
            .Be(100);
        result.PricingServiceResults
            .First()
            .Value.GetInventoryResult.Inventories.First()
            .Value.WarehouseQtyOnHandDtos[1].Name
            .Should()
            .Be("2");
        result.PricingServiceResults
            .First()
            .Value.GetInventoryResult.Inventories.First()
            .Value.WarehouseQtyOnHandDtos[1].QtyOnHand
            .Should()
            .Be(200);
    }

    private static OePricingMultipleV4Response CreateOePricingMultipleV4Response(
        params (
            string erpNumber,
            string unitOfMeasure,
            string warehouse,
            decimal netAvail
        )[] priceOutV2s
    )
    {
        var oePricingMultipleV4Response = new OePricingMultipleV4Response
        {
            Response = new Response
            {
                PriceOutV2Collection = new PriceOutV2Collection
                {
                    PriceOutV2s = new List<PriceOutV2>()
                }
            }
        };

        foreach (var priceOutV2 in priceOutV2s)
        {
            oePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s.Add(
                new PriceOutV2
                {
                    Prod = priceOutV2.erpNumber,
                    Unit = priceOutV2.unitOfMeasure,
                    Whse = priceOutV2.warehouse,
                    NetAvail = priceOutV2.netAvail
                }
            );
        }

        return oePricingMultipleV4Response;
    }

    private void WhenGetProductsIs(
        GetInventoryParameter getInventoryParameter,
        params Product[] products
    )
    {
        this.productHelper
            .Setup(o => o.GetProducts(this.fakeUnitOfWork, getInventoryParameter))
            .Returns(products);
    }

    private void WhenGetWarehouseIs(
        GetInventoryParameter getInventoryParameter,
        WarehouseDto warehouse
    )
    {
        this.warehouseHelper
            .Setup(o => o.GetWarehouse(this.fakeUnitOfWork, getInventoryParameter))
            .Returns(warehouse);
    }

    private void WhenGetWarehouseIs(
        PricingServiceParameter pricingServiceParameter,
        WarehouseDto warehouse
    )
    {
        this.warehouseHelper
            .Setup(o => o.GetWarehouse(this.fakeUnitOfWork, pricingServiceParameter))
            .Returns(warehouse);
    }

    private void WhenExists(Product product)
    {
        this.products.Add(product);
    }
}
