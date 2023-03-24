namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;

using System;
using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class AddProductsToRequest : IPipe<GetCustomerPriceParameter, GetCustomerPriceResult>
{
    public int Order => 200;

    public GetCustomerPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetCustomerPriceParameter parameter,
        GetCustomerPriceResult result
    )
    {
        var productRepository = unitOfWork.GetRepository<Product>();

        result.CustomerPriceRequest.parts = this.GetSalesPartPriceReqDatas(
            parameter.PricingServiceParameters,
            productRepository
        );

        return result;
    }

    private List<salesPartPriceReqData> GetSalesPartPriceReqDatas(
        ICollection<PricingServiceParameter> pricingServiceParameters,
        IRepository<Product> productRepository
    )
    {
        var salesPartPriceReqDatas = new List<salesPartPriceReqData>();

        foreach (var pricingServiceParameter in pricingServiceParameters)
        {
            salesPartPriceReqDatas.AddRange(
                this.GetSalesPartPriceReqDatas(pricingServiceParameter, productRepository)
            );
        }

        return salesPartPriceReqDatas;
    }

    private List<salesPartPriceReqData> GetSalesPartPriceReqDatas(
        PricingServiceParameter pricingServiceParameter,
        IRepository<Product> productRepository
    )
    {
        var salesPartPriceReqDatas = new List<salesPartPriceReqData>();

        var product =
            pricingServiceParameter.Product
            ?? productRepository.Get(pricingServiceParameter.ProductId);
        if (product == null)
        {
            return salesPartPriceReqDatas;
        }

        salesPartPriceReqDatas.Add(
            this.CreateSalesPartPriceReqData(product.ErpNumber, pricingServiceParameter.QtyOrdered)
        );

        return salesPartPriceReqDatas;
    }

    private salesPartPriceReqData CreateSalesPartPriceReqData(string erpNumber, decimal qtyOrdered)
    {
        return new salesPartPriceReqData
        {
            productNo = erpNumber,
            quantity = decimal.ToInt32(qtyOrdered),
            quantitySpecified = true
        };
    }
}
