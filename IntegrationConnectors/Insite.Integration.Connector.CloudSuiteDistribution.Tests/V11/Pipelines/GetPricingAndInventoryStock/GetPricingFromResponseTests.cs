namespace Insite.Integration.Connector.CloudSuiteDistribution.Tests.V11.Pipelines.GetPricingAndInventoryStock;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Insite.Core.Interfaces.Plugins.Pricing;
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
public class GetPricingFromResponseTests
    : BaseForPipeTests<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
{
    private Mock<IWarehouseHelper> warehouseHelper;

    private IList<Product> products;

    public override Type PipeType => typeof(GetPricingFromResponse);

    public override void SetUp()
    {
        this.warehouseHelper = this.container.GetMock<IWarehouseHelper>();

        this.products = new List<Product>();
        this.fakeUnitOfWork.SetupEntityList(this.products);
    }

    [Test]
    public void Order_Is_600()
    {
        this.pipe.Order.Should().Be(600);
    }

    [Test]
    public void Execute_Should_Get_PricingServiceResult_For_PricingServiceParameter_Product()
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

        result.PricingServiceResults.Should().NotBeEmpty();
        result.PricingServiceResults.Should().ContainKey(pricingServiceParameters.First());
        result.PricingServiceResults
            .First()
            .Value.UnitRegularPrice.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s
                    .First()
                    .Price
            );
    }

    [Test]
    public void Execute_Should_Get_PricingServiceResult_For_PricingServiceParameter_Product_And_UnitOfMeasure()
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

        result.PricingServiceResults.Should().NotBeEmpty();
        result.PricingServiceResults.Should().ContainKey(pricingServiceParameters.First());
        result.PricingServiceResults
            .First()
            .Value.UnitRegularPrice.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s[
                    1
                ].Price
            );
    }

    [Test]
    public void Execute_Should_Get_PricingServiceResult_For_PricingServiceParameter_Product_And_Warehouse_Returned_From_WarehouseHelper_GetWarehouse()
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

        result.PricingServiceResults.Should().NotBeEmpty();
        result.PricingServiceResults.Should().ContainKey(pricingServiceParameters.First());
        result.PricingServiceResults
            .First()
            .Value.UnitRegularPrice.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s[
                    1
                ].Price
            );
    }

    [Test]
    public void Execute_Should_Get_PricingServiceResult_For_PricingServiceParameter_ProductId()
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

        result.PricingServiceResults.Should().NotBeEmpty();
        result.PricingServiceResults.Should().ContainKey(pricingServiceParameters.First());
        result.PricingServiceResults
            .First()
            .Value.UnitRegularPrice.Should()
            .Be(
                result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s
                    .First()
                    .Price
            );
    }

    private static OePricingMultipleV4Response CreateOePricingMultipleV4Response(
        params (
            string erpNumber,
            string unitOfMeasure,
            string warehouse,
            decimal price
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
                    Price = priceOutV2.price
                }
            );
        }

        return oePricingMultipleV4Response;
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
