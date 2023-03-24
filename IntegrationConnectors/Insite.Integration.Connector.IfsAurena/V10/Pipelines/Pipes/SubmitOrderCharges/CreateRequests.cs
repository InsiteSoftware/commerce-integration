namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrderCharges;

using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;

public sealed class CreateRequests : IPipe<SubmitOrderChargesParameter, SubmitOrderChargesResult>
{
    private readonly ICustomerHelper customerHelper;

    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public CreateRequests(
        ICustomerHelper customerHelper,
        IntegrationConnectorSettings integrationConnectorSettings
    )
    {
        this.customerHelper = customerHelper;
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 100;

    public SubmitOrderChargesResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderChargesParameter parameter,
        SubmitOrderChargesResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CreateRequests)} Started.");

        var shipTo = this.customerHelper.GetShipTo(unitOfWork, parameter.CustomerOrder);

        result.CustomerOrderChargeRequests = parameter.CustomerOrder.CustomerOrderPromotions
            .Select(
                o =>
                    new CustomerOrderCharge
                    {
                        OrderNo = parameter.ErpOrderNumber,
                        DeliveryAddress = shipTo?.ErpSequence,
                        Contract = parameter.CustomerOrder.DefaultWarehouse?.Name,
                        ChargeAmount = o.Amount,
                        ChargeAmountInclTax = o.Amount,
                        BaseChargeAmount = o.Amount,
                        BaseChargeAmtInclTax = o.Amount,
                        ChargeType = this.integrationConnectorSettings.IfsAurenaPromotionChargeType,
                        Company = this.integrationConnectorSettings.IfsAurenaCompany,
                        ChargeCost = 0,
                        ChargedQty = 1,
                        InvoicedQty = 0,
                        CurrencyRate = 1,
                        StatisticalChargeDiff = 0,
                        SalesUnitMeas = "EA",
                        CollectDb = false,
                        MultipleTaxLines = false,
                        PrintChargeTypeDb = false,
                        PrintCollectChargeDb = false,
                        IntrastatExemptDb = false,
                        UnitChargeDb = false
                    }
            )
            .ToList();

        result.CustomerOrderChargeRequests.Add(
            new CustomerOrderCharge
            {
                OrderNo = parameter.ErpOrderNumber,
                DeliveryAddress = shipTo?.ErpSequence,
                Contract = parameter.CustomerOrder.DefaultWarehouse?.Name,
                ChargeAmount = parameter.CustomerOrder.ShippingCharges,
                ChargeAmountInclTax = parameter.CustomerOrder.ShippingCharges,
                BaseChargeAmount = parameter.CustomerOrder.ShippingCharges,
                BaseChargeAmtInclTax = parameter.CustomerOrder.ShippingCharges,
                ChargeType = this.integrationConnectorSettings.IfsAurenaFreightChargeType,
                Company = this.integrationConnectorSettings.IfsAurenaCompany,
                ChargeCost = 0,
                ChargedQty = 1,
                InvoicedQty = 0,
                CurrencyRate = 1,
                StatisticalChargeDiff = 0,
                SalesUnitMeas = "EA",
                CollectDb = false,
                MultipleTaxLines = false,
                PrintChargeTypeDb = false,
                PrintCollectChargeDb = false,
                IntrastatExemptDb = false,
                UnitChargeDb = false
            }
        );

        parameter.JobLogger?.Debug($"{nameof(CreateRequests)} Finished.");

        return result;
    }
}
