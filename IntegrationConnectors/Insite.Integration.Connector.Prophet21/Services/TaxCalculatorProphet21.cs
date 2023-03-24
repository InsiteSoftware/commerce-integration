namespace Insite.Integration.Connector.Prophet21.Services;

using Insite.Common.Dependencies;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Plugins.Tax;
using Insite.Data.Entities;
using Insite.Integration.Attributes;
using Insite.Integration.Enums;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[DependencyName(nameof(IntegrationConnectorType.Prophet21))]
[IntegrationConnector(IntegrationConnectorType = IntegrationConnectorType.Prophet21)]
public sealed class TaxCalculatorProphet21 : ITaxCalculator
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    private readonly IProphet21IntegrationConnectionProvider prophet21IntegrationConnectionProvider;

    public TaxCalculatorProphet21(
        IPipeAssemblyFactory pipeAssemblyFactory,
        IDependencyLocator dependencyLocator
    )
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
        this.prophet21IntegrationConnectionProvider =
            dependencyLocator.GetInstance<IProphet21IntegrationConnectionProvider>();
    }

    public void CalculateTax(OriginAddress originAddress, CustomerOrder customerOrder)
    {
        var integrationConnection =
            this.prophet21IntegrationConnectionProvider.GetIntegrationConnection(null);

        var getCartSummaryParameter = new GetCartSummaryParameter
        {
            CustomerOrder = customerOrder,
            IntegrationConnection = integrationConnection
        };

        var getCartSummaryResult = this.pipeAssemblyFactory.ExecutePipeline(
            getCartSummaryParameter,
            new GetCartSummaryResult()
        );

        customerOrder.StateTax = getCartSummaryResult.TaxAmount;
        customerOrder.TaxCalculated = getCartSummaryResult.TaxCalculated;
    }

    public void PostTax(OriginAddress originAddress, CustomerOrder customerOrder)
    {
        // Don't need to do anything
    }
}
