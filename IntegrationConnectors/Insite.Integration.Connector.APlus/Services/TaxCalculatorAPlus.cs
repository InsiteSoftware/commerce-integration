namespace Insite.Integration.Connector.APlus.Services;

using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Plugins.Tax;
using Insite.Data.Entities;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[DependencyName(nameof(IntegrationConnectorType.APlus))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.APlus)]
public sealed class TaxCalculatorAPlus : ITaxCalculator
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public TaxCalculatorAPlus(IPipeAssemblyFactory pipeAssemblyFactory)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public void CalculateTax(OriginAddress originAddress, CustomerOrder customerOrder)
    {
        var createOrderParameter = new CreateOrderParameter
        {
            CustomerOrder = customerOrder,
            IsOrderSubmit = false
        };
        var createOrderResult = this.pipeAssemblyFactory.ExecutePipeline(
            createOrderParameter,
            new CreateOrderResult()
        );

        customerOrder.StateTax = createOrderResult.TaxAmount;
        customerOrder.TaxCalculated = createOrderResult.TaxCalculated;
    }

    public void PostTax(OriginAddress originAddress, CustomerOrder customerOrder)
    {
        // Don't need to do anything
    }
}
