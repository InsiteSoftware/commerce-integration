namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.CustomerPaymentMethod;

using System;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.CustomerPaymentMethod;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.CustomerPaymentMethod;
using NUnit.Framework;

[TestFixture]
public class GetCustomerProfileIdFromResponseTests
    : BaseForPipeTests<CustomerPaymentMethodParameter, CustomerPaymentMethodResult>
{
    public override Type PipeType => typeof(GetCustomerProfileIdFromResponse);

    public override void SetUp() { }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_CustomerProfileId_From_CustomerPaymentMethodResponse()
    {
        var result = this.GetDefaultResult();
        result.CustomerPaymentMethodResponse = new CustomerPaymentMethod()
        {
            CustomerProfileID = "Id123"
        };

        result = this.RunExecute(result);

        Assert.AreEqual(
            result.CustomerPaymentMethodResponse.CustomerProfileID,
            result.CustomerProfileId
        );
    }
}
