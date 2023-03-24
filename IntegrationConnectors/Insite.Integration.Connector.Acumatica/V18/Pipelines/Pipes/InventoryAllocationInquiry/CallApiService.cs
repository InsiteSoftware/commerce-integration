namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.InventoryAllocationInquiry;

using System;
using Insite.Common.Dependencies;
using Insite.Common.Logging;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.Acumatica.Services;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;

public sealed class CallApiService
    : IPipe<InventoryAllocationInquiryParameter, InventoryAllocationInquiryResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 300;

    public InventoryAllocationInquiryResult Execute(
        IUnitOfWork unitOfWork,
        InventoryAllocationInquiryParameter parameter,
        InventoryAllocationInquiryResult result
    )
    {
        var acumaticaApiService = this.dependencyLocator
            .GetInstance<IAcumaticaApiServiceFactory>()
            .GetAcumaticaApiService(parameter.IntegrationConnection);

        acumaticaApiService.Login();

        foreach (var inventoryAllocationInquiryRequest in result.InventoryAllocationInquiryRequests)
        {
            try
            {
                var inventoryAllocationInquiryResponse =
                    acumaticaApiService.InventoryAllocationInquiry(
                        inventoryAllocationInquiryRequest
                    );

                result.InventoryAllocationInquiryResponses.Add(inventoryAllocationInquiryResponse);
            }
            catch (Exception exception)
            {
                LogHelper.For(this).Error(exception.Message);
            }
        }

        acumaticaApiService.Logout();

        return result;
    }
}
