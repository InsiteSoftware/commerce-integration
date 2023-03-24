namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.LineItemPriceAndAvailability;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class CreateInitialRequestTests
    : BaseForPipeTests<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Create_New_LineItemPriceAndAvailabilityRequest()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.LineItemPriceAndAvailabilityRequest);
        Assert.AreEqual("GetAvail", result.LineItemPriceAndAvailabilityRequest.Name);
    }
}
