namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.GetOrderNotes;

using System;
using System.Linq;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.GetOrderNotes;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class GetOrderNotesTests : BaseForPipeTests<GetOrderNotesParameter, GetOrderNotesResult>
{
    public override Type PipeType => typeof(GetOrderNotes);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Return_Empty_List_When_Notes_Is_Blank()
    {
        var parameter = new GetOrderNotesParameter { Notes = string.Empty };

        var result = this.RunExecute(parameter);

        CollectionAssert.IsEmpty(result.LineItemInfos);
    }

    [Test]
    public void Execute_Should_Return_LineItemInfos_Split_On_ThirtyOne_Characters_Without_Splitting_On_A_Word()
    {
        var notesOne = "one1 one1 one1 one1 one1 one1";
        var notesTwo = "two2 two2 two2 two2 two2 two2";
        var notesThree = "tree tree tree tree tree tree";

        var parameter = new GetOrderNotesParameter
        {
            Notes = $"{notesOne} {notesTwo} {notesThree}",
            LineItemType = "M"
        };

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.LineItemInfos.Any(o => o.ItemDesc1 == notesOne && o.ItemDesc2 == notesTwo)
        );
        Assert.IsTrue(
            result.LineItemInfos.Any(
                o => o.ItemDesc1 == notesThree && string.IsNullOrEmpty(o.ItemDesc2)
            )
        );
        Assert.IsTrue(result.LineItemInfos.All(o => o.LineItemType == "M"));
        Assert.IsTrue(result.LineItemInfos.All(o => o.ItemNumber == string.Empty));
    }
}
