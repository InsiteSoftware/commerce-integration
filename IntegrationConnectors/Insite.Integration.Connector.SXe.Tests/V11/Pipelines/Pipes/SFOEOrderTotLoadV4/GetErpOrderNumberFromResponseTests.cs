namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFOEOrderTotLoadV4;

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
                Response = new Response
                {
                    OrdLoadHdrDataCollection = new OrdLoadHdrDataCollection
                    {
                        OrdLoadHdrDatas = new List<OrdLoadHdrData>
                        {
                            new OrdLoadHdrData { Orderno = 1234, Ordersuf = 0 }
                        }
                    }
                }
            }
        };
    }
}
