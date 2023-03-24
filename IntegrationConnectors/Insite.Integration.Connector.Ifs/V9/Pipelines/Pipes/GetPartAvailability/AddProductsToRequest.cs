namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetPartAvailability;

using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class AddProductsToRequest
    : IPipe<GetPartAvailabilityParameter, GetPartAvailabilityResult>
{
    public int Order => 200;

    public GetPartAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        GetPartAvailabilityParameter parameter,
        GetPartAvailabilityResult result
    )
    {
        result.PartAvailabilityRequest.partsAvailabile = this.GetPartAvailabilityReqDatas(
            unitOfWork,
            parameter.GetInventoryParameter
        );

        return result;
    }

    private List<partAvailabilityReqData> GetPartAvailabilityReqDatas(
        IUnitOfWork unitOfWork,
        GetInventoryParameter getInventoryParameter
    )
    {
        var productRepository = unitOfWork.GetRepository<Product>();
        var partAvailabilityReqDatas = new List<partAvailabilityReqData>();

        if (getInventoryParameter == null)
        {
            return partAvailabilityReqDatas;
        }

        foreach (var product in getInventoryParameter.Products)
        {
            partAvailabilityReqDatas.Add(this.CreatePartAvailabilityReqData(product.ErpNumber));
        }

        foreach (var productId in getInventoryParameter.ProductIds)
        {
            var product = productRepository.Get(productId);
            if (product == null)
            {
                continue;
            }

            partAvailabilityReqDatas.Add(this.CreatePartAvailabilityReqData(product.ErpNumber));
        }

        return partAvailabilityReqDatas;
    }

    private partAvailabilityReqData CreatePartAvailabilityReqData(string erpNumber)
    {
        return new partAvailabilityReqData
        {
            productNo = erpNumber,
            wantedQuantity = 1000, // api only returns available quantity up to requested amount set here
            wantedQuantitySpecified = true
        };
    }
}
