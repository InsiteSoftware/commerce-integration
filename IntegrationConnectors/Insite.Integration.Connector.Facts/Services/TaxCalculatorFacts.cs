namespace Insite.Integration.Connector.Facts.Services;

using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Plugins.Tax;
using Insite.Data.Entities;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;

[DependencyName(nameof(IntegrationConnectorType.FACTS))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.FACTS)]
public sealed class TaxCalculatorFacts : ITaxCalculator
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public TaxCalculatorFacts(IPipeAssemblyFactory pipeAssemblyFactory)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public void CalculateTax(OriginAddress originAddress, CustomerOrder customerOrder)
    {
        var orderTotalParameter = new OrderTotalParameter { CustomerOrder = customerOrder };
        var orderTotalResult = this.pipeAssemblyFactory.ExecutePipeline(
            orderTotalParameter,
            new OrderTotalResult()
        );

        customerOrder.StateTax = orderTotalResult.TaxAmount;
        customerOrder.TaxCalculated = orderTotalResult.TaxCalculated;
    }

    public void PostTax(OriginAddress originAddress, CustomerOrder customerOrder)
    {
        // Don't need to do anything
    }
}
