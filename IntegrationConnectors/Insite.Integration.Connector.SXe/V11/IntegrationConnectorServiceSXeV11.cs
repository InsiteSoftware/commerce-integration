namespace Insite.Integration.Connector.SXe.V11;

using System.Collections.Generic;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;

[DependencyName(nameof(SXeVersion.Eleven))]
internal sealed class IntegrationConnectorServiceSXeV11 : IIntegrationConnectorServiceSXe
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public IntegrationConnectorServiceSXeV11(IPipeAssemblyFactory pipeAssemblyFactory)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public void CalculateTax(CustomerOrder customerOrder)
    {
        var getSFOEOrderTotLoadV4Parameter = new SFOEOrderTotLoadV4Parameter
        {
            CustomerOrder = customerOrder,
            IsOrderSubmit = false
        };

        var getSFOEOrderTotLoadV4Result = this.pipeAssemblyFactory.ExecutePipeline(
            getSFOEOrderTotLoadV4Parameter,
            new SFOEOrderTotLoadV4Result()
        );

        customerOrder.StateTax = getSFOEOrderTotLoadV4Result.TaxAmount;
        customerOrder.TaxCalculated = getSFOEOrderTotLoadV4Result.TaxCalculated;
    }

    public GetAgingBucketsResult GetAgingBuckets(GetAgingBucketsParameter getAgingBucketsParameter)
    {
        var getSFCustomerSummaryParameter = new SFCustomerSummaryParameter
        {
            GetAgingBucketsParameter = getAgingBucketsParameter
        };

        var getSFCustomerSummaryResult = this.pipeAssemblyFactory.ExecutePipeline(
            getSFCustomerSummaryParameter,
            new SFCustomerSummaryResult()
        );

        return getSFCustomerSummaryResult.GetAgingBucketsResult;
    }

    public GetInventoryResult GetInventory(
        GetInventoryParameter getInventoryParameter,
        IntegrationConnection integrationConnection
    )
    {
        var oePricingMultipleV5Parameter = new OEPricingMultipleV4Parameter
        {
            GetInventoryParameter = getInventoryParameter,
            IntegrationConnection = integrationConnection
        };

        var oePricingMultipleV5Result = this.pipeAssemblyFactory.ExecutePipeline(
            oePricingMultipleV5Parameter,
            new OEPricingMultipleV4Result()
        );

        return oePricingMultipleV5Result.GetInventoryResult;
    }

    public IDictionary<PricingServiceParameter, PricingServiceResult> GetPrice(
        ICollection<PricingServiceParameter> pricingServiceParameters,
        IntegrationConnection integrationConnection
    )
    {
        var oePricingMultipleV5Parameter = new OEPricingMultipleV4Parameter
        {
            PricingServiceParameters = pricingServiceParameters,
            IntegrationConnection = integrationConnection
        };

        var oePricingMultipleV5Result = this.pipeAssemblyFactory.ExecutePipeline(
            oePricingMultipleV5Parameter,
            new OEPricingMultipleV4Result()
        );

        return oePricingMultipleV5Result.PricingServiceResults;
    }
}
