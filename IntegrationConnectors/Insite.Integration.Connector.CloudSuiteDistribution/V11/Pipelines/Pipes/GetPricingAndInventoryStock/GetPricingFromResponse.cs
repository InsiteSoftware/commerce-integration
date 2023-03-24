namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetPricingAndInventoryStock;

using System;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Plugins.Utilities;
using Insite.Data.Entities;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.OePricingMultipleV4;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Data.Entities.Dtos;

public sealed class GetPricingFromResponse
    : IPipe<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
{
    private readonly IWarehouseHelper warehouseHelper;

    private readonly ICurrencyFormatProvider currencyFormatProvider;

    public GetPricingFromResponse(
        IWarehouseHelper warehouseHelper,
        ICurrencyFormatProvider currencyFormatProvider
    )
    {
        this.warehouseHelper = warehouseHelper;
        this.currencyFormatProvider = currencyFormatProvider;
    }

    public int Order => 600;

    public GetPricingAndInventoryStockResult Execute(
        IUnitOfWork unitOfWork,
        GetPricingAndInventoryStockParameter parameter,
        GetPricingAndInventoryStockResult result
    )
    {
        var productRepository = unitOfWork.GetRepository<Product>();
        var warehouse = this.warehouseHelper.GetWarehouse(
            unitOfWork,
            parameter.PricingServiceParameters.FirstOrDefault()
        );
        var currency = SiteContext.Current.CurrencyDto;

        foreach (var pricingServiceParameter in parameter.PricingServiceParameters)
        {
            var product =
                pricingServiceParameter.Product
                ?? productRepository.Get(pricingServiceParameter.ProductId);
            var priceOutV2 = GetPriceOutV2ByErpNumberUnitOfMeasureAndWarehouse(
                result,
                product,
                pricingServiceParameter.UnitOfMeasure,
                warehouse
            );
            if (priceOutV2 == null)
            {
                continue;
            }

            result.PricingServiceResults[pricingServiceParameter] = new PricingServiceResult
            {
                UnitRegularPrice = priceOutV2.Price.Value,
                UnitRegularPriceDisplay = this.currencyFormatProvider.GetString(
                    priceOutV2.Price.Value,
                    currency
                )
            };
        }

        return result;
    }

    private static PriceOutV2 GetPriceOutV2ByErpNumberUnitOfMeasureAndWarehouse(
        GetPricingAndInventoryStockResult result,
        Product product,
        string unitOfMeasure,
        WarehouseDto warehouse
    )
    {
        if (result.OePricingMultipleV4Response?.Response?.PriceOutV2Collection?.PriceOutV2s == null)
        {
            return null;
        }

        var priceOutV2sByErpNumber =
            result.OePricingMultipleV4Response.Response.PriceOutV2Collection.PriceOutV2s.Where(
                o => o.Prod.EqualsIgnoreCase(product.ErpNumber)
            );

        var priceOutV2sByUnitOfMeasure = priceOutV2sByErpNumber.Where(
            o => o.Unit.EqualsIgnoreCase(unitOfMeasure)
        );
        if (!priceOutV2sByUnitOfMeasure.Any())
        {
            priceOutV2sByUnitOfMeasure = priceOutV2sByErpNumber;
        }

        var priceOutV2sByWarehouse = priceOutV2sByUnitOfMeasure.Where(
            o => o.Whse.EqualsIgnoreCase(warehouse?.Name)
        );
        if (!priceOutV2sByWarehouse.Any())
        {
            priceOutV2sByWarehouse = priceOutV2sByUnitOfMeasure;
        }

        return priceOutV2sByWarehouse.OrderBy(o => o.SeqNo).FirstOrDefault();
    }
}
