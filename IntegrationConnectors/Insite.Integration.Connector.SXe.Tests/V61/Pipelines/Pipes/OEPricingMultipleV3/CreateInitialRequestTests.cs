namespace Insite.Integration.Connector.SXe.Tests.V6.Pipelines.Pipes.OEPricingMultipleV3;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.OEPricingMultipleV3;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;

[TestFixture]
public class CreateInitialRequestTests
    : BaseForPipeTests<OEPricingMultipleV3Parameter, OEPricingMultipleV3Result>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Create_New_OEPricingMultipleV3Request()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.OEPricingMultipleV3Request);
        Assert.IsTrue(result.OEPricingMultipleV3Request.sendFullQtyOnOrder);
    }
}
