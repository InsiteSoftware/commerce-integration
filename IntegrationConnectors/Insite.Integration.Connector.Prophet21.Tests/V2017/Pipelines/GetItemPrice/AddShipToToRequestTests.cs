namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetItemPrice;

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

[TestFixture]
public class AddShipToToRequestTests : BaseForPipeTests<GetItemPriceParameter, GetItemPriceResult>
{
    private IList<Customer> customers;

    public override Type PipeType => typeof(AddShipToToRequest);

    public override void SetUp()
    {
        this.customers = new List<Customer>();
        this.fakeUnitOfWork.SetupEntityList(this.customers);
    }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Return_Result_When_ShipToId_Is_Null()
    {
        Assert.DoesNotThrow(() => this.RunExecute());
    }

    [Test]
    public void Execute_Should_Throw_Exception_When_ShipToId_Not_Found_In_Repository()
    {
        var parameter = new GetItemPriceParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { ShipToId = Guid.NewGuid() }
            }
        };

        Assert.Throws<ArgumentException>(() => this.RunExecute(parameter));
    }

    [Test]
    public void Execute_Should_Return_Result_When_ShipTo_ErpSequence_Is_Empty()
    {
        var customer = Some.Customer().Build();

        var parameter = new GetItemPriceParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { ShipToId = customer.Id }
            }
        };

        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.That(result.GetItemPriceRequest.Request.ShipToID, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Set_Ship_To_Id_From_BillTo_PricingCustomer_ErpSequence()
    {
        var customer = Some.Customer()
            .WithPricingCustomer(Some.Customer().WithErpSequence("456"))
            .WithErpSequence("123")
            .Build();

        var parameter = new GetItemPriceParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { ShipToId = customer.Id }
            }
        };

        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customer.PricingCustomer.ErpSequence,
            result.GetItemPriceRequest.Request.ShipToID
        );
    }

    [Test]
    public void Execute_Should_Set_Ship_To_Id_From_ShipTo_ErpSequence()
    {
        var customer = Some.Customer().WithErpSequence("123").Build();

        var parameter = new GetItemPriceParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { ShipToId = customer.Id }
            }
        };

        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customer.ErpSequence, result.GetItemPriceRequest.Request.ShipToID);
    }

    protected override GetItemPriceResult GetDefaultResult()
    {
        return new GetItemPriceResult
        {
            GetItemPriceRequest = new GetItemPrice { Request = new Request() }
        };
    }

    protected void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }
}
