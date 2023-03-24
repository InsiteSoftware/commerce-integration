namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;

using System;
using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class AddLineItemInfosToRequest : IPipe<CreateOrderParameter, CreateOrderResult>
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddLineItemInfosToRequest(
        IPipeAssemblyFactory pipeAssemblyFactory,
        IntegrationConnectorSettings integrationConnectorSettings
    )
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 500;

    public CreateOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateOrderParameter parameter,
        CreateOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddLineItemInfosToRequest)} Started.");

        var lineItemInfos = new List<RequestLineItemInfo>();

        // order lines
        foreach (var orderLine in parameter.CustomerOrder.OrderLines)
        {
            lineItemInfos.Add(
                new RequestLineItemInfo
                {
                    ItemNumber = orderLine.Product.ErpNumber,
                    OrderQty = orderLine.QtyOrdered.ToString(),
                    UnitOfMeasure = orderLine.UnitOfMeasure,
                    WarehouseId = orderLine.Warehouse?.Name ?? string.Empty,
                    ActualSellPrice = orderLine.UnitNetPrice.ToString(),
                    LineItemType = "I"
                }
            );

            var getOrderLineNotesParameter = new GetOrderNotesParameter
            {
                Notes = orderLine.Notes,
                LineItemType = "M"
            };
            var getOrderLineNotesResult = this.pipeAssemblyFactory.ExecutePipeline(
                getOrderLineNotesParameter,
                new GetOrderNotesResult()
            );
            if (getOrderLineNotesResult.ResultCode != ResultCode.Success)
            {
                result.ResultCode = getOrderLineNotesResult.ResultCode;
                result.SubCode = getOrderLineNotesResult.SubCode;
                result.Messages = getOrderLineNotesResult.Messages;
                return result;
            }

            lineItemInfos.AddRange(getOrderLineNotesResult.LineItemInfos);
        }

        // order notes
        var getOrderNotesParameter = new GetOrderNotesParameter
        {
            Notes = parameter.CustomerOrder.Notes,
            LineItemType = "/"
        };
        var getOrderNotesResult = this.pipeAssemblyFactory.ExecutePipeline(
            getOrderNotesParameter,
            new GetOrderNotesResult()
        );
        if (getOrderNotesResult.ResultCode != ResultCode.Success)
        {
            result.ResultCode = getOrderNotesResult.ResultCode;
            result.SubCode = getOrderNotesResult.SubCode;
            result.Messages = getOrderNotesResult.Messages;
            return result;
        }

        lineItemInfos.AddRange(getOrderNotesResult.LineItemInfos);

        // shipping and handling
        var shippingAndHandling =
            parameter.CustomerOrder.ShippingCharges + parameter.CustomerOrder.HandlingCharges;
        if (
            shippingAndHandling > 0
            && !this.integrationConnectorSettings.APlusFreightChargeCode.IsBlank()
        )
        {
            lineItemInfos.Add(
                new RequestLineItemInfo
                {
                    ItemNumber = string.Empty,
                    LineItemType = "/",
                    OverridePrice = "Y",
                    ChargeType = this.integrationConnectorSettings.APlusFreightChargeCode,
                    ActualSellPrice = shippingAndHandling.ToString()
                }
            );
        }

        if (result.CreateOrderRequest.Orders[0].OrderDetail == null)
        {
            result.CreateOrderRequest.Orders[0].OrderDetail = new RequestOrderDetail
            {
                LineItemInfo = new List<RequestLineItemInfo>()
            };
        }

        result.CreateOrderRequest.Orders[0].OrderDetail.LineItemInfo.AddRange(lineItemInfos);

        parameter.JobLogger?.Debug($"{nameof(AddLineItemInfosToRequest)} Finished.");

        return result;
    }
}
