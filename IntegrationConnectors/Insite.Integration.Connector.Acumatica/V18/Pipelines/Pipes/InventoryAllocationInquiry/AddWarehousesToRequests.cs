namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.InventoryAllocationInquiry;

using System.Collections.Generic;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities.Dtos;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.InventoryAllocationInquiry;

public sealed class AddWarehousesToRequests
    : IPipe<InventoryAllocationInquiryParameter, InventoryAllocationInquiryResult>
{
    public int Order => 200;

    public InventoryAllocationInquiryResult Execute(
        IUnitOfWork unitOfWork,
        InventoryAllocationInquiryParameter parameter,
        InventoryAllocationInquiryResult result
    )
    {
        var inventoryAllocationInquiryRequests = new List<InventoryAllocationInquiry>();
        var warehouses = this.GetWarehouses(unitOfWork, parameter.GetInventoryParameter);

        foreach (var inventoryAllocationInquiryRequest in result.InventoryAllocationInquiryRequests)
        {
            foreach (var warehouse in warehouses)
            {
                inventoryAllocationInquiryRequests.Add(
                    new InventoryAllocationInquiry
                    {
                        InventoryID = inventoryAllocationInquiryRequest.InventoryID,
                        WarehouseID = warehouse.Name
                    }
                );
            }
        }

        result.InventoryAllocationInquiryRequests = inventoryAllocationInquiryRequests;

        return result;
    }

    private IEnumerable<WarehouseDto> GetWarehouses(
        IUnitOfWork unitOfWork,
        GetInventoryParameter getInventoryParameter
    )
    {
        var warehouses = unitOfWork
            .GetTypedRepository<IWarehouseRepository>()
            .GetCachedWarehouses();

        if (getInventoryParameter.GetWarehouses)
        {
            return warehouses;
        }
        else
        {
            var warehouse =
                warehouses.FirstOrDefault(o => o.Id == getInventoryParameter.WarehouseId)
                ?? warehouses.FirstOrDefault(o => o.Id == SiteContext.Current.WarehouseDto.Id);

            return new List<WarehouseDto> { warehouse };
        }
    }
}
