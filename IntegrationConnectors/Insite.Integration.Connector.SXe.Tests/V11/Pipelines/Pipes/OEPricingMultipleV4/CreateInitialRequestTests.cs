namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.OEPricingMultipleV4;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.OEPricingMultipleV4;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;

[TestFixture]
public class CreateInitialRequestTests
    : BaseForPipeTests<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Create_New_OEPricingMultipleV4Request()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.OEPricingMultipleV4Request);
        Assert.IsTrue(result.OEPricingMultipleV4Request.Request.SendFullQtyOnOrder);
    }
}
