namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFOEOrderTotLoadV4;

[TestFixture]
public class GetTaxAmountFromResponseTests
    : BaseForPipeTests<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    public override Type PipeType => typeof(GetTaxAmountFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_1200()
    {
        Assert.AreEqual(1200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Not_Get_Tax_Amount_When_No_OutOutTotals()
    {
        var result = new SFOEOrderTotLoadV4Result
        {
            SFOEOrderTotLoadV4Response = new SFOEOrderTotLoadV4Response { }
        };

        result = this.RunExecute(result);

        Assert.AreEqual(0, result.TaxAmount);
        Assert.AreEqual(false, result.TaxCalculated);
    }

    [Test]
    public void Execute_Should_Get_Tax_Amount()
    {
        var result = new SFOEOrderTotLoadV4Result
        {
            SFOEOrderTotLoadV4Response = new SFOEOrderTotLoadV4Response
            {
                Response = new Response
                {
                    OrdTotDataCollection = new OrdTotDataCollection
                    {
                        OrdTotDatas = new List<OrdTotData> { new OrdTotData { Tottaxamt = 12 } }
                    }
                }
            }
        };

        result = this.RunExecute(result);

        Assert.AreEqual(12, result.TaxAmount);
        Assert.AreEqual(true, result.TaxCalculated);
    }
}
