namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFOEOrderTotLoadV4;

[TestFixture]
public class AddInInLinesToRequestTests
    : BaseForPipeTests<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    public override Type PipeType => typeof(AddInInLinesToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_900()
    {
        Assert.AreEqual(900, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Order_Lines_From_Order_Notes()
    {
        var customerOrder = Some.CustomerOrder().WithNotes("TestNotes").Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "/",
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas
                .First()
                .Itemnumber
        );
        Assert.AreEqual(
            "cp",
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas
                .First()
                .Lineitemtype
        );
        Assert.AreEqual(
            customerOrder.Notes,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas
                .First()
                .Itemdesc1
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

        Assert.AreEqual(
            "&",
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas[
                1
            ].Itemnumber
        );
        Assert.AreEqual(
            "cp",
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas[
                1
            ].Lineitemtype
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().Notes,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas[
                1
            ].Itemdesc1
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

        var parameter = new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderLines.First().UnitNetPrice,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas
                .First()
                .Actualsellprice
        );
        Assert.AreEqual(
            0,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas
                .First()
                .Cost
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().Product.ErpNumber,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas
                .First()
                .Itemnumber
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().QtyOrdered,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas
                .First()
                .Orderqty
        );
        Assert.AreEqual(
            customerOrder.OrderLines.First().UnitOfMeasure,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas
                .First()
                .Unitofmeasure
        );
    }

    [Test]
    public void Execute_Should_Get_Order_Line_Warehouse_From_Order_Line_Warehouse()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.OrderLine().With(Some.Product()).With(Some.Warehouse().WithName("ABC")))
            .Build();

        var parameter = new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.OrderLines.First().Warehouse.Name,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas
                .First()
                .Warehouseid
        );
    }

    [Test]
    public void Execute_Should_Get_Order_Line_Warehouse_From_Customer_Order_Warehouse_When_Order_Line_Warehouse_Is_Null()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.OrderLine().With(Some.Product()))
            .WithDefaultWarehouse(Some.Warehouse().WithName("DEF"))
            .Build();

        var parameter = new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.DefaultWarehouse.Name,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas
                .First()
                .Warehouseid
        );
    }

    [Test]
    public void Execute_Should_Get_Warehouse_As_Empty_When_Order_Line_Warehouse_And_Customer_Order_Warehouse_Are_Null()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.OrderLine().With(Some.Product()))
            .Build();

        var parameter = new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            string.Empty,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas
                .First()
                .Warehouseid
        );
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

        Assert.AreEqual(
            1,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas[0].Seqno
        );
        Assert.AreEqual(
            2,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas[1].Seqno
        );
        Assert.AreEqual(
            3,
            result.SFOEOrderTotLoadV4Request.Request.InputLineDataCollection.InputLineDatas[2].Seqno
        );
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

    protected override SFOEOrderTotLoadV4Result GetDefaultResult()
    {
        return new SFOEOrderTotLoadV4Result
        {
            SFOEOrderTotLoadV4Request = new SFOEOrderTotLoadV4Request { Request = new Request() }
        };
    }
}
