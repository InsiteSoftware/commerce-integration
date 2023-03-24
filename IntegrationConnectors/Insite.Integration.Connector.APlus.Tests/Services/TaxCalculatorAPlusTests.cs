namespace Insite.Integration.Connector.APlus.Tests.Services;

using Moq;
using NUnit.Framework;

using Insite.Core.Plugins.Pipelines;
using Insite.Core.TestHelpers;
using Insite.Core.TestHelpers.Builders;
using Insite.Data.Entities;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;
using Insite.Integration.Connector.APlus.Services;

[TestFixture]
public class TaxCalculatorAPlusTests
{
    private TaxCalculatorAPlus taxCalculatorAPlus;

    private Mock<IPipeAssemblyFactory> pipeAssemblyFactory;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        this.pipeAssemblyFactory = container.GetMock<IPipeAssemblyFactory>();

        this.taxCalculatorAPlus = container.Resolve<TaxCalculatorAPlus>();
    }

    [Test]
    public void Execute_Should_Call_Execute_Pipeline_With_Customer_Order()
    {
        var customerOrder = Some.CustomerOrder().Build();

        var getCreateOrderResult = new CreateOrderResult();

        this.WhenExecutePipelineIs(getCreateOrderResult);

        this.taxCalculatorAPlus.CalculateTax(null, customerOrder);

        this.VerifyExecutePipelineWasCalled(customerOrder);
    }

    [TestCase(12, true)]
    [TestCase(0, false)]
    public void Execute_Should_Set_Customer_Order_State_Tax_From_GetCreateOrderResult_Tax_Amount(
        decimal taxAmount,
        bool taxCalculated
    )
    {
        var customerOrder = Some.CustomerOrder().Build();

        var getCreateOrderResult = new CreateOrderResult
        {
            TaxAmount = taxAmount,
            TaxCalculated = taxCalculated
        };

        this.WhenExecutePipelineIs(getCreateOrderResult);

        this.taxCalculatorAPlus.CalculateTax(null, customerOrder);

        Assert.AreEqual(getCreateOrderResult.TaxAmount, customerOrder.StateTax);
        Assert.AreEqual(getCreateOrderResult.TaxCalculated, customerOrder.TaxCalculated);
    }

    private void WhenExecutePipelineIs(CreateOrderResult createOrderResult)
    {
        this.pipeAssemblyFactory
            .Setup(
                o =>
                    o.ExecutePipeline(
                        It.IsAny<CreateOrderParameter>(),
                        It.IsAny<CreateOrderResult>()
                    )
            )
            .Returns(createOrderResult);
    }

    private void VerifyExecutePipelineWasCalled(CustomerOrder customerOrder)
    {
        this.pipeAssemblyFactory.Verify(
            o =>
                o.ExecutePipeline(
                    It.Is<CreateOrderParameter>(p => p.CustomerOrder == customerOrder),
                    It.IsAny<CreateOrderResult>()
                ),
            Times.Once
        );
    }
}
