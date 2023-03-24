namespace Insite.Integration.Connector.SXe.Tests.V6.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

[TestFixture]
public class AddInInLinesToRequestTests
    : BaseForPipeTests<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    public override Type PipeType => typeof(AddInInLinesToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_800()
    {
        Assert.AreEqual(800, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Order_Lines_From_Order_Notes()
    {
        var customerOrder = Some.CustomerOrder().WithNotes("TestNotes").Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual("/", result.SFOEOrderTotLoadV2Request.arrayInline[0].itemNumber);
        Assert.AreEqual("cp", result.SFOEOrderTotLoadV2Request.arrayInline[0].lineItemType);
        Assert.AreEqual(
            customerOrder.Notes,
            result.SFOEOrderTotLoadV2Request.arrayInline[0].itemDescription1
        );
    }

    [Test]
    public void Execute_Should_Get_Order_Lines_From_Order_Line_Notes()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.OrderLine().With(Some.Product().WithErpNumber("ABC123")).WithNotes("TestNotes")
            )
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual("&", result.SFOEOrderTotLoadV2Request.arrayInline[1].itemNumber);
        Assert.AreEqual("cp", result.SFOEOrderTotLoadV2Request.arrayInline[1].lineItemType);
        Assert.AreEqual(
            customerOrder.OrderLines.First().Notes,
            result.SFOEOrderTotLoadV2Request.arrayInline[1].itemDescription1
        );
    }

    [Test]
    public void Execute_Should_Get_Order_Lines_From_Customer_Order_Lines()
    {
        var customerOrder = Some.CustomerOrder()
            .With(
                Some.OrderLine()
                    .WithUnitNetPrice(10)
                    .With(Some.Product().WithErpNumber("ABC123"))
                    .WithQtyOrdered(2)
                    .WithUnitOfMeasure("CS")
            )
            .Build();

        var parameter = new SFOEOrderTotLoadV2Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderLines.First().UnitNetPrice,
            result.SFOEOrderTotLoadV2Request.arrayInline[0].actualSellPrice
        );
        Assert.AreEqual(0, result.SFOEOrderTotLoadV2Request.arrayInline[0].cost);
        Assert.AreEqual(
            customerOrder.OrderLines.First().Product.ErpNumber,
            result.SFOEOrderTotLoadV2Request.arrayInline[0].itemNumber
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().QtyOrdered,
            result.SFOEOrderTotLoadV2Request.arrayInline[0].orderQty
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().UnitOfMeasure,
            result.SFOEOrderTotLoadV2Request.arrayInline[0].unitOfMeasure
        );
    }

    [Test]
    public void Execute_Should_Get_Order_Line_Warehouse_From_Order_Line_Warehouse()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.OrderLine().With(Some.Product()).With(Some.Warehouse().WithName("ABC")))
            .Build();

        var parameter = new SFOEOrderTotLoadV2Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderLines.First().Warehouse.Name,
            result.SFOEOrderTotLoadV2Request.arrayInline[0].warehouseID
        );
    }

    [Test]
    public void Execute_Should_Get_Order_Line_Warehouse_From_Customer_Order_Warehouse_When_Order_Line_Warehouse_Is_Null()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.OrderLine().With(Some.Product()))
            .WithDefaultWarehouse(Some.Warehouse().WithName("DEF"))
            .Build();

        var parameter = new SFOEOrderTotLoadV2Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.DefaultWarehouse.Name,
            result.SFOEOrderTotLoadV2Request.arrayInline[0].warehouseID
        );
    }

    [Test]
    public void Execute_Should_Get_Warehouse_As_Empty_When_Order_Line_Warehouse_And_Customer_Order_Warehouse_Are_Null()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.OrderLine().With(Some.Product()))
            .Build();

        var parameter = new SFOEOrderTotLoadV2Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(string.Empty, result.SFOEOrderTotLoadV2Request.arrayInline[0].warehouseID);
    }

    [Test]
    public void Execute_Should_Set_Order_Line_Sequence_Number()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.OrderLine().With(Some.Product()))
            .With(Some.OrderLine().With(Some.Product()))
            .With(Some.OrderLine().With(Some.Product()))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(1, result.SFOEOrderTotLoadV2Request.arrayInline[0].sequenceNumber);
        Assert.AreEqual(2, result.SFOEOrderTotLoadV2Request.arrayInline[1].sequenceNumber);
        Assert.AreEqual(3, result.SFOEOrderTotLoadV2Request.arrayInline[2].sequenceNumber);
    }

    [Test]
    public void SplitNotes_Should_Return_Notes_Split_On_Maximum_Sixty_Characters_And_White_Space_Only()
    {
        var notesOne = "NotesNotesNotesNotesNotesNotesNotesNotes";
        var notesTwo = "NotesNotesNotesNotesNotesNotesNotesNotesNotes";
        var notes = $"{notesOne} {notesTwo}";

        var result = (List<string>)this.RunPrivateMethod("SplitNotes", notes);

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(notesOne, result[0]);
        Assert.AreEqual(notesTwo, result[1]);
    }

    protected override SFOEOrderTotLoadV2Result GetDefaultResult()
    {
        return new SFOEOrderTotLoadV2Result
        {
            SFOEOrderTotLoadV2Request = new SFOEOrderTotLoadV2Request { }
        };
    }
}
