namespace Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetPricingAndInventoryStock;

using System.Collections.Generic;
using System.Linq;
using Insite.Common.Helpers;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.IonApi.Models.OePricingMultipleV4;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;

public sealed class CreateRequest
    : IPipe<GetPricingAndInventoryStockParameter, GetPricingAndInventoryStockResult>
{
    private readonly ICustomerHelper customerHelper;

    private readonly IProductHelper productHelper;

    private readonly IWarehouseHelper warehouseHelper;

    public CreateRequest(
        ICustomerHelper customerHelper,
        IProductHelper productHelper,
        IWarehouseHelper warehouseHelper
    )
    {
        this.customerHelper = customerHelper;
        this.productHelper = productHelper;
        this.warehouseHelper = warehouseHelper;
    }

    public int Order => 100;

    public GetPricingAndInventoryStockResult Execute(
        IUnitOfWork unitOfWork,
        GetPricingAndInventoryStockParameter parameter,
        GetPricingAndInventoryStockResult result
    )
    {
        var billTo = this.customerHelper.GetBillTo(
            unitOfWork,
            parameter.PricingServiceParameters.FirstOrDefault()
        );
        var shipTo = this.customerHelper.GetShipTo(
            unitOfWork,
            parameter.PricingServiceParameters.FirstOrDefault()
        );
        var priceInV2s = this.GetPriceInV2sForPricingServiceParameters(
                unitOfWork,
                parameter.PricingServiceParameters
            )
            .Concat(
                this.GetPriceInV2sForGetInventoryParameter(
                    unitOfWork,
                    parameter.GetInventoryParameter
                )
            )
            .ToList();

        int.TryParse(parameter.IntegrationConnection?.SystemNumber, out var companyNumber);

#pragma warning disable CS0618 // Type or member is obsolete
        result.OePricingMultipleV4Request = new OePricingMultipleV4Request
        {
            Request = new Request
            {
                CompanyNumber = companyNumber,
                OperatorInit = parameter.IntegrationConnection?.LogOn,
                OperatorPassword = EncryptionHelper.DecryptAes(
                    parameter.IntegrationConnection?.Password
                ),
                SendFullQtyOnOrder = true,
                CustomerNumber = billTo?.ErpNumber,
                ShipTo = shipTo?.ErpSequence,
                PriceInV2Collection = new PriceInV2Collection { PriceInV2s = priceInV2s }
            }
        };
#pragma warning restore

        return result;
    }

    private IEnumerable<PriceInV2> GetPriceInV2sForPricingServiceParameters(
        IUnitOfWork unitOfWork,
        ICollection<PricingServiceParameter> pricingServiceParameters
    )
    {
        var productRepository = unitOfWork.GetRepository<Product>();
        var warehouse = this.warehouseHelper.GetWarehouse(
            unitOfWork,
            pricingServiceParameters.FirstOrDefault()
        );
        var priceInV2s = new List<PriceInV2>();

        foreach (var pricingServiceParameter in pricingServiceParameters)
        {
            var product =
                pricingServiceParameter.Product
                ?? productRepository.Get(pricingServiceParameter.ProductId);
            var qtyOrdered =
                pricingServiceParameter.QtyOrdered <= 0 ? 1 : pricingServiceParameter.QtyOrdered;

            priceInV2s.Add(
                new PriceInV2
                {
                    Prod = product.ErpNumber,
                    QtyOrd = qtyOrdered,
                    Unit = pricingServiceParameter.UnitOfMeasure,
                    Whse = warehouse?.Name
                }
            );
        }

        return priceInV2s;
    }

    private IEnumerable<PriceInV2> GetPriceInV2sForGetInventoryParameter(
        IUnitOfWork unitOfWork,
        GetInventoryParameter getInventoryParameter
    )
    {
        var products = this.productHelper.GetProducts(unitOfWork, getInventoryParameter);
        var warehouse = this.warehouseHelper.GetWarehouse(unitOfWork, getInventoryParameter);

        return products.Select(
            o =>
                new PriceInV2
                {
                    Prod = o.ErpNumber,
                    QtyOrd = 1,
                    Unit = o.UnitOfMeasure,
                    Whse = warehouse?.Name
                }
        );
    }
}
