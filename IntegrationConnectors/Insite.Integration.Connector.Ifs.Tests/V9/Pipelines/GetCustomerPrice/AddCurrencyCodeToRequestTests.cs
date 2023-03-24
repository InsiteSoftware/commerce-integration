namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.GetCustomerPrice;

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

[TestFixture]
public class AddCurrencyCodeToRequestTests
    : BaseForPipeTests<GetCustomerPriceParameter, GetCustomerPriceResult>
{
    public override Type PipeType => typeof(AddCurrencyCodeToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Currency_Code_From_Pricing_Service_Parameters()
    {
        var parameter = new GetCustomerPriceParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { CurrencyCode = "USD" }
            }
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            parameter.PricingServiceParameters.First().CurrencyCode,
            result.CustomerPriceRequest.currencyCode
        );
    }

    protected override GetCustomerPriceResult GetDefaultResult()
    {
        return new GetCustomerPriceResult { CustomerPriceRequest = new customerPriceRequest() };
    }
}
