namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetPartAvailability;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Integration.Connector.Ifs.Services;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;

public sealed class CallApiService : IPipe<GetPartAvailabilityParameter, GetPartAvailabilityResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 600;

    public GetPartAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        GetPartAvailabilityParameter parameter,
        GetPartAvailabilityResult result
    )
    {
        var ifsApiService = this.dependencyLocator
            .GetInstance<IIfsApiServiceFactory>()
            .GetIfsApiService(parameter.IntegrationConnection);

        MakeTheCall(ifsApiService, result, result.PartAvailabilityRequest);

        if (parameter.GetInventoryParameter?.GetWarehouses ?? false)
        {
            MakeTheCallForAllWarehouses(ifsApiService, unitOfWork, result);
        }

        return result;
    }

    private static void MakeTheCall(
        IIfsApiService ifsApiService,
        GetPartAvailabilityResult result,
        partAvailabilityRequest partAvailabilityRequest
    )
    {
        var partAvailabilityResponse = ifsApiService.GetPartAvailability(partAvailabilityRequest);

        if (!string.IsNullOrEmpty(partAvailabilityResponse?.errorText))
        {
            HandleErrorResponse(result, partAvailabilityResponse);
        }
        else
        {
            HandleSuccessResponse(result, partAvailabilityResponse);
        }
    }

    private static void MakeTheCallForAllWarehouses(
        IIfsApiService ifsApiService,
        IUnitOfWork unitOfWork,
        GetPartAvailabilityResult result
    )
    {
        var warehouses = unitOfWork
            .GetTypedRepository<IWarehouseRepository>()
            .GetCachedWarehouses()
            .Where(
                o =>
                    !o.Name.Equals(
                        result.PartAvailabilityRequest.site,
                        StringComparison.OrdinalIgnoreCase
                    )
            );

        foreach (var warehouseDto in warehouses)
        {
            var partAvailabilityRequest = CopyPartAvailabilityRequest(
                result.PartAvailabilityRequest,
                warehouseDto.Name
            );

            MakeTheCall(ifsApiService, result, partAvailabilityRequest);
        }
    }

    private static partAvailabilityRequest CopyPartAvailabilityRequest(
        partAvailabilityRequest partAvailabilityRequest,
        string warehouse
    )
    {
        return new partAvailabilityRequest
        {
            addressId = partAvailabilityRequest.addressId,
            customerNo = partAvailabilityRequest.customerNo,
            custOwnAddressId = partAvailabilityRequest.custOwnAddressId,
            partsAvailabile = partAvailabilityRequest.partsAvailabile,
            site = warehouse
        };
    }

    private static void HandleErrorResponse(
        GetPartAvailabilityResult result,
        partAvailabilityResponse partAvailabilityResponse
    )
    {
        if (result.Messages == null)
        {
            result.Messages = new List<ResultMessage>();
        }

        result.ResultCode = ResultCode.Error;
        result.SubCode = SubCode.GeneralFailure;
        result.Messages.Add(new ResultMessage { Message = partAvailabilityResponse.errorText });
    }

    private static void HandleSuccessResponse(
        GetPartAvailabilityResult result,
        partAvailabilityResponse partAvailabilityResponse
    )
    {
        if (result.PartAvailabilityResponse == null)
        {
            result.PartAvailabilityResponse = partAvailabilityResponse;
        }
        else
        {
            result.PartAvailabilityResponse.partsAvailabile.AddRange(
                partAvailabilityResponse.partsAvailabile
            );
        }
    }
}
