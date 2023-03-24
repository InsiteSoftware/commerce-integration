namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.CreateCustomerOrder;

using System;
using System.Linq;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.CreateCustomerOrder;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.CreateCustomerOrder;

[TestFixture]
public class AddProductsToRequestTests
    : BaseForPipeTests<CreateCustomerOrderParameter, CreateCustomerOrderResult>
{
    public override Type PipeType => typeof(AddProductsToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_CustomerOrderLines_With_OrderLines()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.OrderLine()
                    .WithLine(1)
                    .With(Some.Product().WithErpNumber("ABC123"))
                    .WithQtyOrdered(2)
                    .WithUnitRegularPrice(12)
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderLines.First().Line.ToString(),
            result.CustomerOrder.lines.First().lineNo
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().Product.ErpNumber,
            result.CustomerOrder.lines.First().catalogNo
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().QtyOrdered,
            result.CustomerOrder.lines.First().buyQtyDue
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().UnitRegularPrice,
            result.CustomerOrder.lines.First().saleUnitPrice
        );
    }

    [TestCase("EA", 12)]
    [TestCase("CS", 1)]
    public void Execute_Should_Populate_OrderLine_ConvFactor(
        string unitOfMeasure,
        decimal qtyPerBaseUnitOfMeasure
    )
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.OrderLine()
                    .WithLine(1)
                    .WithUnitOfMeasure(unitOfMeasure)
                    .With(
                        Some.Product()
                            .WithErpNumber("ABC123")
                            .With(
                                Some.ProductUnitOfMeasure()
                                    .WithUnitOfMeasure("EA")
                                    .WithQtyPerBaseUnitOfMeasure(12)
                            )
                    )
                    .WithQtyOrdered(2)
                    .WithUnitRegularPrice(12)
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(qtyPerBaseUnitOfMeasure, result.CustomerOrder.lines.First().convFactor);
    }

    [Test]
    public void Execute_Should_Populate_NoteText_To_Fifty_Characters()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.OrderLine()
                    .WithLine(1)
                    .WithNotes(new string('*', 2001))
                    .With(Some.Product().WithErpNumber("ABC123"))
                    .WithQtyOrdered(2)
                    .WithUnitRegularPrice(12)
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderLines.First().Notes.Substring(0, 2000),
            result.CustomerOrder.lines.First().noteText
        );
    }

    protected override CreateCustomerOrderResult GetDefaultResult()
    {
        return new CreateCustomerOrderResult { CustomerOrder = new customerOrder() };
    }
}
