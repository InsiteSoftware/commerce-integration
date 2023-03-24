namespace Insite.Integration.Connector.Prophet21.Tests.Services;

using Insite.Common.Dependencies;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.TestHelpers;
using Insite.Core.TestHelpers.Builders;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.Services;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;
using Moq;
using NUnit.Framework;

[TestFixture]
public class TaxCalculatorProphet21Tests
{
    private TaxCalculatorProphet21 taxCalculatorAPlus;

    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    private Mock<IProphet21IntegrationConnectionProvider> prophet21IntegrationConnectionProvider;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        this.pipeAssemblyFactory = container.GetMock<IPipeAssemblyFactory>();
        this.prophet21IntegrationConnectionProvider =
            container.GetMock<IProphet21IntegrationConnectionProvider>();

        container
            .GetMock<IDependencyLocator>()
            .Setup(o => o.GetInstance<IProphet21IntegrationConnectionProvider>())
            .Returns(this.prophet21IntegrationConnectionProvider.Object);

        this.taxCalculatorAPlus = container.Resolve<TaxCalculatorProphet21>();
    }

    [TestCase(12, true)]
    [TestCase(0, false)]
    public void Execute_Should_Set_Customer_Order_State_Tax_From_GetCreateOrderResult_Tax_Amount(
        decimal taxAmount,
        bool taxCalculated
    )
    {
        var customerOrder = Some.CustomerOrder().Build();

        var getCartSummaryResult = new GetCartSummaryResult
        {
            TaxAmount = taxAmount,
            TaxCalculated = taxCalculated
        };

        this.WhenExecutePipelineIs(getCartSummaryResult);

        this.taxCalculatorAPlus.CalculateTax(null, customerOrder);

        Assert.AreEqual(getCartSummaryResult.TaxAmount, customerOrder.StateTax);
        Assert.AreEqual(getCartSummaryResult.TaxCalculated, customerOrder.TaxCalculated);
    }

    private void WhenExecutePipelineIs(GetCartSummaryResult createCartSummaryResult)
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.IsAny<GetCartSummaryParameter>(),
                        It.IsAny<GetCartSummaryResult>()
                    )
            )
            .Returns(createCartSummaryResult);
    }

    private void VerifyExecutePipelineWasCalled(CustomerOrder customerOrder)
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<GetCartSummaryParameter>(p => p.CustomerOrder == customerOrder),
                    It.IsAny<GetCartSummaryResult>()
                ),
            Times.Once
        );
    }

    [Test]
    public void Execute_Should_Call_Execute_Pipeline_With_Customer_Order()
    {
        var customerOrder = Some.CustomerOrder().Build();

        var getCartSummaryResult = new GetCartSummaryResult();

        this.WhenExecutePipelineIs(getCartSummaryResult);

        this.taxCalculatorAPlus.CalculateTax(null, customerOrder);

        this.VerifyExecutePipelineWasCalled(customerOrder);
    }
}
