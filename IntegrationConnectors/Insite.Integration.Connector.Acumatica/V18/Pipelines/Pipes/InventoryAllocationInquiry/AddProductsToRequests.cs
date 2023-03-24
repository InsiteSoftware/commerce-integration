namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.InventoryAllocationInquiry;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.InventoryAllocationInquiry;

public sealed class AddProductsToRequests
    : IPipe<InventoryAllocationInquiryParameter, InventoryAllocationInquiryResult>
{
    public int Order => 100;

    public InventoryAllocationInquiryResult Execute(
        IUnitOfWork unitOfWork,
        InventoryAllocationInquiryParameter parameter,
        InventoryAllocationInquiryResult result
    )
    {
        if (parameter.GetInventoryParameter == null)
        {
            return result;
        }

        foreach (var product in parameter.GetInventoryParameter.Products)
        {
            result.InventoryAllocationInquiryRequests.Add(
                new InventoryAllocationInquiry { InventoryID = product.ErpNumber }
            );
        }

        var productRepository = unitOfWork.GetRepository<Product>();
        foreach (
            var productId in parameter.GetInventoryParameter.ProductIds.Where(
                o => !parameter.GetInventoryParameter.Products.Any(p => p.Id == o)
            )
        )
        {
            var product = productRepository.Get(productId);
            if (product == null)
            {
                continue;
            }

            result.InventoryAllocationInquiryRequests.Add(
                new InventoryAllocationInquiry { InventoryID = product.ErpNumber }
            );
        }

        return result;
    }
}
