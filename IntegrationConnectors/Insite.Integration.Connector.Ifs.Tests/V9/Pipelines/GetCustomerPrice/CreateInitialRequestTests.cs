namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.GetCustomerPrice;

using System;

using NUnit.Framework;

using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

[TestFixture]
public class CreateInitialRequestTests
    : BaseForPipeTests<GetCustomerPriceParameter, GetCustomerPriceResult>
{
    public override Type PipeType => typeof(CreateInitialRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        Assert.AreEqual(100, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Create_New_CustomerPriceRequest()
    {
        var result = this.RunExecute();

        Assert.IsNotNull(result.CustomerPriceRequest);
    }
}
