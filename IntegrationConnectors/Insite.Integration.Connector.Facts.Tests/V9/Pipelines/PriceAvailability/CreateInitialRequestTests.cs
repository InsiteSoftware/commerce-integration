namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.PriceAvailability;

using System;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;
using NUnit.Framework;

[TestFixture]
public class CreateInitialRequestTests
    : BaseForPipeTests<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_Initial_Request()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.PriceAvailabilityRequest);
        Assert.IsNotNull(result.PriceAvailabilityRequest.Request);
        Assert.AreEqual("PriceAvailability", result.PriceAvailabilityRequest.Request.RequestID);
    }
}
