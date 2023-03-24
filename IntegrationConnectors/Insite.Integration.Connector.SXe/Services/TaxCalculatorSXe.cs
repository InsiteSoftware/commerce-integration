namespace Insite.Integration.Connector.SXe.Services;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Tax;
using Insite.Data.Entities;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;

[DependencyName(nameof(IntegrationConnectorType.SXe))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.SXe)]
public sealed class TaxCalculatorSXe : ITaxCalculator
{
    private readonly IDependencyLocator dependencyLocator;

    public TaxCalculatorSXe(IDependencyLocator dependencyLocator)
    {
        this.dependencyLocator = dependencyLocator;
    }

    public void CalculateTax(OriginAddress originAddress, CustomerOrder customerOrder)
    {
        this.dependencyLocator
            .GetInstance<IIntegrationConnectorServiceSXeFactory>()
            .GetIntegrationConnectorServiceSXe()
            .CalculateTax(customerOrder);
    }

    public void PostTax(OriginAddress originAddress, CustomerOrder customerOrder)
    {
        // Don't need to do anything
    }
}
