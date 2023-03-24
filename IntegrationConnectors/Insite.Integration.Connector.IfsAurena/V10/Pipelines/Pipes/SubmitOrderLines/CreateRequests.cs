namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrderLines;

using System.Linq;
using Insite.Common.Providers;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;

public sealed class CreateRequests : IPipe<SubmitOrderLinesParameter, SubmitOrderLinesResult>
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

    public SubmitOrderLinesResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderLinesParameter parameter,
        SubmitOrderLinesResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(CreateRequests)} Started.");

        var billTo = this.customerHelper.GetBillTo(unitOfWork, parameter.CustomerOrder);
        var shipTo = this.customerHelper.GetShipTo(unitOfWork, parameter.CustomerOrder);
        var defaultDeliveryDate = DateTimeProvider.Current.Now.AddDays(1);

        result.CustomerOrderLineRequests = parameter.CustomerOrder.OrderLines
            .Select(
                o =>
                    new CustomerOrderLine
                    {
                        OrderNo = parameter.ErpOrderNumber,
                        Identity1 = parameter.ErpOrderNumber,
                        DeliverToCustomerNo = billTo?.ErpNumber,
                        ShipAddrNo = shipTo?.ErpSequence,
                        Contract = parameter.CustomerOrder.DefaultWarehouse?.Name,
                        PlannedDeliveryDate =
                            parameter.CustomerOrder.RequestedDeliveryDate ?? defaultDeliveryDate,
                        WantedDeliveryDate =
                            parameter.CustomerOrder.RequestedDeliveryDate ?? defaultDeliveryDate,
                        TargetDate =
                            parameter.CustomerOrder.RequestedDeliveryDate ?? defaultDeliveryDate,
                        CatalogNo = o.Product?.ErpNumber,
                        PartNo = o.Product?.ProductCode,
                        SalesUnitMeas = o.UnitOfMeasure,
                        PriceUnitMeas = o.UnitOfMeasure,
                        BuyQtyDue = o.QtyOrdered,
                        DesiredQty = o.QtyOrdered,
                        BaseSaleUnitPrice = o.UnitNetPrice,
                        BaseUnitPriceInclTax = o.UnitNetPrice,
                        SaleUnitPrice = o.UnitNetPrice,
                        UnitPriceInclTax = o.UnitNetPrice,
                        Cost = o.UnitCost,
                        SupplyCode = this.integrationConnectorSettings.IfsAurenaSupplyCode,
                        ConvFactor = 1,
                        CurrencyRate = 1,
                        Discount = 0,
                        OrderDiscount = 0,
                        PriceConvFactor = 1,
                        QtyAssigned = 0,
                        QtyShort = 0,
                        RevisedQtyDue = 1,
                        CloseTolerance = 0,
                        PartPrice = 0,
                        CatalogType = "InventoryPart",
                        ConsignmentStock = "NoConsignmentStock",
                        ChargedItem = "ChargedItem",
                        CreateSmObjectOption = "DoNotCreateSMObject",
                        DefaultAddrFlag = "Yes",
                        AddrFlag = "No",
                        StagedBilling = "NotStagedBilling",
                        TaxLiability = "TAX",
                        SmConnection = "NotConnected",
                        PriceSource = "SalesPart",
                        PriceFreeze = "Free",
                        ConfigurationId = "*",
                        ReleasePlanning = "NotReleased",
                        Source = "CUSTOMERORDER",
                        FetchTaxFromDefaults = "TRUE",
                        NoteText = o.Notes
                    }
            )
            .ToList();

        parameter.JobLogger?.Debug($"{nameof(CreateRequests)} Finished.");

        return result;
    }
}
