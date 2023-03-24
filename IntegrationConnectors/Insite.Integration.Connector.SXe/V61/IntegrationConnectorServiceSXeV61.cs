namespace Insite.Integration.Connector.SXe.V61;

using System.Collections.Generic;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.Plugins.Customer;
using Insite.Core.Plugins.Inventory;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.Services;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;

[DependencyName(nameof(SXeVersion.SixOne))]
internal sealed class IntegrationConnectorServiceSXeV61 : IIntegrationConnectorServiceSXe
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public IntegrationConnectorServiceSXeV61(IPipeAssemblyFactory pipeAssemblyFactory)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public void CalculateTax(CustomerOrder customerOrder)
    {
        var getSFOEOrderTotLoadV2Parameter = new SFOEOrderTotLoadV2Parameter
        {
            CustomerOrder = customerOrder,
            IsOrderSubmit = false
        };

        var getSFOEOrderTotLoadV2Result = this.pipeAssemblyFactory.ExecutePipeline(
            getSFOEOrderTotLoadV2Parameter,
            new SFOEOrderTotLoadV2Result()
        );

        customerOrder.StateTax = getSFOEOrderTotLoadV2Result.TaxAmount;
        customerOrder.TaxCalculated = getSFOEOrderTotLoadV2Result.TaxCalculated;
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
        var getOEPricingMultipleV3Parameter = new OEPricingMultipleV3Parameter()
        {
            GetInventoryParameter = getInventoryParameter,
            IntegrationConnection = integrationConnection
        };

        var getOEPricingMultipleV3Result = this.pipeAssemblyFactory.ExecutePipeline(
            getOEPricingMultipleV3Parameter,
            new OEPricingMultipleV3Result()
        );

        return getOEPricingMultipleV3Result.GetInventoryResult;
    }

    public IDictionary<PricingServiceParameter, PricingServiceResult> GetPrice(
        ICollection<PricingServiceParameter> pricingServiceParameters,
        IntegrationConnection integrationConnection
    )
    {
        var getOEPricingMultipleV3Parameter = new OEPricingMultipleV3Parameter
        {
            PricingServiceParameters = pricingServiceParameters,
            IntegrationConnection = integrationConnection
        };

        var getOEPricingMultipleV3Result = this.pipeAssemblyFactory.ExecutePipeline(
            getOEPricingMultipleV3Parameter,
            new OEPricingMultipleV3Result()
        );

        return getOEPricingMultipleV3Result.PricingServiceResults;
    }
}
