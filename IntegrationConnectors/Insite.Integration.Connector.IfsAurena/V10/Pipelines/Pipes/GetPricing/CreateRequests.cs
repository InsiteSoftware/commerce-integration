namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.GetPricing;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class CreateRequests : IPipe<GetPricingParameter, GetPricingResult>
{
    private readonly ICustomerHelper customerHelper;

    private readonly IWarehouseHelper warehouseHelper;

    public CreateRequests(ICustomerHelper billToHelper, IWarehouseHelper warehouseHelper)
    {
        this.customerHelper = billToHelper;
        this.warehouseHelper = warehouseHelper;
    }

    public int Order => 100;

    public GetPricingResult Execute(
        IUnitOfWork unitOfWork,
        GetPricingParameter parameter,
        GetPricingResult result
    )
    {
        var productRepository = unitOfWork.GetRepository<Product>();
        var billTo = this.customerHelper.GetBillTo(
            unitOfWork,
            parameter.PricingServiceParameters.FirstOrDefault()
        );
        var warehouse = this.warehouseHelper.GetWarehouse(
            unitOfWork,
            parameter.PricingServiceParameters.FirstOrDefault()
        );
        var currencyCode = parameter.PricingServiceParameters.FirstOrDefault()?.CurrencyCode;

        foreach (var pricingServiceParameter in parameter.PricingServiceParameters)
        {
            var product =
                pricingServiceParameter.Product
                ?? productRepository.Get(pricingServiceParameter.ProductId);
            var qtyOrdered =
                pricingServiceParameter.QtyOrdered <= 0 ? 1 : pricingServiceParameter.QtyOrdered;

            result.PriceQueryRequests.Add(
                new PriceQuery
                {
                    CatalogNo = product.ErpNumber,
                    SalesQty = qtyOrdered,
                    PriceQty = qtyOrdered,
                    CustomerNo = billTo?.ErpNumber,
                    Contract = warehouse?.Name,
                    CurrencyCode = currencyCode,
                    UsePriceInclTax = false
                }
            );
        }

        return result;
    }
}
