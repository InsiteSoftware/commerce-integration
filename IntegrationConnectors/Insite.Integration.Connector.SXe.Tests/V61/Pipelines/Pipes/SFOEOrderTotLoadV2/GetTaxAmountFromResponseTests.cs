namespace Insite.Integration.Connector.SXe.Tests.V6.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

[TestFixture]
public class GetTaxAmountFromResponseTests
    : BaseForPipeTests<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    public override Type PipeType => typeof(GetTaxAmountFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_1100()
    {
        Assert.AreEqual(1100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Not_Get_Tax_Amount_When_No_OutOutTotals()
    {
        var result = new SFOEOrderTotLoadV2Result
        {
            SFOEOrderTotLoadV2Response = new SFOEOrderTotLoadV2Response { }
        };

        result = this.RunExecute(result);

        Assert.AreEqual(0, result.TaxAmount);
        Assert.AreEqual(false, result.TaxCalculated);
    }

    [Test]
    public void Execute_Should_Get_Tax_Amount()
    {
        var result = new SFOEOrderTotLoadV2Result
        {
            SFOEOrderTotLoadV2Response = new SFOEOrderTotLoadV2Response
            {
                arrayOuttotal = new List<SFOEOrderTotLoadV2outputOuttotal>
                {
                    new SFOEOrderTotLoadV2outputOuttotal { salesTaxAmount = 12 }
                }
            }
        };

        result = this.RunExecute(result);

        Assert.AreEqual(12, result.TaxAmount);
        Assert.AreEqual(true, result.TaxCalculated);
    }
}
