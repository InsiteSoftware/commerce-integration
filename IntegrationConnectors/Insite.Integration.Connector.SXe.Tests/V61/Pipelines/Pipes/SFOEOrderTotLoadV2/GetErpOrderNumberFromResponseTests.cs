namespace Insite.Integration.Connector.SXe.Tests.V6.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

[TestFixture]
public class GetErpOrderNumberFromResponseTests
    : BaseForPipeTests<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    public override Type PipeType => typeof(GetErpOrderNumberFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_1000()
    {
        Assert.AreEqual(1000, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Erp_Order_Number()
    {
        var result = this.RunExecute();

        Assert.AreEqual("1234-00", result.ErpOrderNumber);
    }

    protected override SFOEOrderTotLoadV2Parameter GetDefaultParameter()
    {
        return new SFOEOrderTotLoadV2Parameter
        {
            CustomerOrder = Some.CustomerOrder().Build(),
            IsOrderSubmit = true
        };
    }

    protected override SFOEOrderTotLoadV2Result GetDefaultResult()
    {
        return new SFOEOrderTotLoadV2Result
        {
            SFOEOrderTotLoadV2Response = new SFOEOrderTotLoadV2Response
            {
                arrayOutheader = new List<SFOEOrderTotLoadV2outputOutheader>()
                {
                    new SFOEOrderTotLoadV2outputOutheader { orderNumber = 1234, orderSuffix = 0 }
                }
            }
        };
    }
}
