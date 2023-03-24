namespace Insite.Integration.Connector.SXe.Tests.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;

[TestFixture]
public class GetErpOrderNumberFromResponseTests
    : BaseForPipeTests<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    public override Type PipeType => typeof(GetErpOrderNumberFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_1100()
    {
        Assert.AreEqual(1100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Erp_Order_Number()
    {
        var result = this.RunExecute();

        Assert.AreEqual("1234-00", result.ErpOrderNumber);
    }

    protected override SFOEOrderTotLoadV4Parameter GetDefaultParameter()
    {
        return new SFOEOrderTotLoadV4Parameter
        {
            CustomerOrder = Some.CustomerOrder().Build(),
            IsOrderSubmit = true
        };
    }

    protected override SFOEOrderTotLoadV4Result GetDefaultResult()
    {
        return new SFOEOrderTotLoadV4Result
        {
            SFOEOrderTotLoadV4Response = new SFOEOrderTotLoadV4Response
            {
                Outoutheader = new List<Outoutheader3>()
                {
                    new Outoutheader3 { OrderNumber = 1234, OrderSuffix = 0 }
                }
            }
        };
    }
}
