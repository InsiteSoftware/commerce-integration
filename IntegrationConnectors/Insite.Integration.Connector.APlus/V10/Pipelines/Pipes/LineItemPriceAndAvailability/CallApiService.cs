namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;
using Insite.Integration.Connector.APlus.Services;

public sealed class CallApiService
    : IPipe<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    private readonly IDependencyLocator dependencyLocator;

    public CallApiService(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public int Order => 500;

    public LineItemPriceAndAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        LineItemPriceAndAvailabilityParameter parameter,
        LineItemPriceAndAvailabilityResult result
    )
    {
        try
        {
            result.LineItemPriceAndAvailabilityResponse = this.dependencyLocator
                .GetInstance<IAPlusApiServiceFactory>()
                .GetAPlusApiService(parameter.IntegrationConnection)
                .LineItemPriceAndAvailability(result.LineItemPriceAndAvailabilityRequest);
        }
        catch (Exception exception)
        {
            result.ResultCode = ResultCode.Error;
            result.SubCode = SubCode.GeneralFailure;
            result.Messages.Add(new ResultMessage { Message = exception.Message });
        }

        return result;
    }

    private LineItemPriceAndAvailabilityResponse SimulateOEPricingMultipleV4(
        LineItemPriceAndAvailabilityRequest lineItemPriceAndAvailabilityRequest
    )
    {
        var random = new Random();

        var lineItemPriceAndAvailabilityResponseItemAvailability =
            lineItemPriceAndAvailabilityRequest.Items
                .Select(
                    o =>
                        new ResponseItemAvailability
                        {
                            Item = new ResponseItem { ItemNumber = o.ItemNumber },
                            WarehouseInfo = new List<ResponseWarehouseInfo>
                            {
                                new ResponseWarehouseInfo
                                {
                                    PricingUOM = o.UnitofMeasure,
                                    Qty = random.Next(16, 100).ToString(),
                                    Warehouse = o.WarehouseID,
                                    Price = random.Next(1, 1000).ToString(),
                                }
                            }
                        }
                )
                .ToList();

        return new LineItemPriceAndAvailabilityResponse
        {
            ItemAvailability = lineItemPriceAndAvailabilityResponseItemAvailability
        };
    }
}
