namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;

using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class AddOrderChargesToRequest
    : IPipe<CreateCustomerOrderParameter, CreateCustomerOrderResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddOrderChargesToRequest(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 500;

    public CreateCustomerOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateCustomerOrderParameter parameter,
        CreateCustomerOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddOrderChargesToRequest)} Started.");

        result.CustomerOrder.charges = this.GetOrderCharges(parameter.CustomerOrder);

        parameter.JobLogger?.Debug($"{nameof(AddOrderChargesToRequest)} Finished.");

        return result;
    }

    private List<orderCharge> GetOrderCharges(CustomerOrder customerOrder)
    {
        var orderCharges = new List<orderCharge>();

        if (customerOrder.CustomerOrderPromotions.Any())
        {
            orderCharges.AddRange(this.GetCustomerOrderPromotionsOrderCharges(customerOrder));
        }

        if (GetChargeAmount(customerOrder) > 0)
        {
            orderCharges.Add(this.GetShippingAndHandlingOrderCharge(customerOrder));
        }

        return orderCharges.Any() ? orderCharges : null;
    }

    private List<orderCharge> GetCustomerOrderPromotionsOrderCharges(CustomerOrder customerOrder)
    {
        var orderCharges = new List<orderCharge>();

        foreach (var customerOrderPromotion in customerOrder.CustomerOrderPromotions)
        {
            orderCharges.Add(
                this.GetCustomerOrderPromotionOrderCharge(customerOrder, customerOrderPromotion)
            );
        }

        return orderCharges;
    }

    private orderCharge GetCustomerOrderPromotionOrderCharge(
        CustomerOrder customerOrder,
        CustomerOrderPromotion customerOrderPromotion
    )
    {
        var orderCharge = new orderCharge
        {
            chargeType = this.integrationConnectorSettings.IfsChargeTypePromotion,
            chargeAmount = GetChargeAmount(customerOrderPromotion),
            chargeAmountSpecified = true,
            chargedQty = 1,
            chargedQtySpecified = true
        };

        var orderLine = customerOrder.OrderLines.FirstOrDefault(
            o => o.Id == customerOrderPromotion.OrderLineId
        );
        if (orderLine != null)
        {
            orderCharge.chargeAmount = orderCharge.chargeAmount / orderLine.QtyOrdered;

            orderCharge.lineNo = orderLine.Line.ToString();
            orderCharge.chargedQty = orderLine.QtyOrdered;
        }

        return orderCharge;
    }

    private orderCharge GetShippingAndHandlingOrderCharge(CustomerOrder customerOrder)
    {
        return new orderCharge
        {
            chargeType = this.integrationConnectorSettings.IfsChargeTypeFreight,
            chargeAmount = GetChargeAmount(customerOrder),
            chargeAmountSpecified = true,
            chargedQty = 1,
            chargedQtySpecified = true
        };
    }

    private static decimal GetChargeAmount(CustomerOrderPromotion customerOrderPromotion)
    {
        return (customerOrderPromotion.Amount ?? 0) * -1;
    }

    private static decimal GetChargeAmount(CustomerOrder customerOrder)
    {
        return customerOrder.ShippingCharges + customerOrder.HandlingCharges;
    }
}
