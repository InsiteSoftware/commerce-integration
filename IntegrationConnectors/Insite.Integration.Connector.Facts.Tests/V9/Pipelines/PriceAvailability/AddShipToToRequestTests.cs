namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.PriceAvailability;

using System;
using System.Collections.Generic;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class AddShipToToRequestTests
    : BaseForPipeTests<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    private const string DefaultShipToNumber = "SAME";

    private IList<Customer> customers;

    public override Type PipeType => typeof(AddShipToToRequest);

    public override void SetUp()
    {
        this.customers = new List<Customer>();
        this.fakeUnitOfWork.SetupEntityList(this.customers);
    }

    [Test]
    public void Order_Is_500()
    {
        Assert.AreEqual(500, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Return_Result_When_ShipToId_Is_Null()
    {
        Assert.DoesNotThrow(() => this.RunExecute());
    }

    [Test]
    public void Execute_Should_Throw_Exception_When_ShipToId_Not_Found_In_Repository()
    {
        var parameter = this.CreatePriceAvailabilityParameter(Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => this.RunExecute(parameter));
    }

    [Test]
    public void Execute_Should_Set_ShipToNumber_From_DefaultShipToNumber_When_ShipTo_ErpSequence_Is_Empty()
    {
        var customer = Some.Customer().Build();

        var parameter = this.CreatePriceAvailabilityParameter(customer.Id);

        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.PriceAvailabilityRequest.Request.Items.TrueForAll(
                o => o.ShipToNumber.Equals(DefaultShipToNumber)
            )
        );
    }

    [Test]
    public void Execute_Should_Set_ShipToNumber_From_BillTo_PricingCustomer_ErpSequence()
    {
        var customer = Some.Customer()
            .WithPricingCustomer(Some.Customer().WithErpSequence("456"))
            .WithErpSequence("123")
            .Build();

        var parameter = this.CreatePriceAvailabilityParameter(customer.Id);

        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.PriceAvailabilityRequest.Request.Items.TrueForAll(
                o => o.ShipToNumber.Equals(customer.PricingCustomer.ErpSequence)
            )
        );
    }

    [Test]
    public void Execute_Should_Set_ShipToNumber_From_ShipTo_ErpSequence()
    {
        var customer = Some.Customer().WithErpSequence("123").Build();

        var parameter = this.CreatePriceAvailabilityParameter(customer.Id);

        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.PriceAvailabilityRequest.Request.Items.TrueForAll(
                o => o.ShipToNumber.Equals(customer.ErpSequence)
            )
        );
    }

    protected override PriceAvailabilityResult GetDefaultResult()
    {
        return new PriceAvailabilityResult
        {
            PriceAvailabilityRequest = new PriceAvailabilityRequest
            {
                Request = new Request
                {
                    Items = new List<RequestItem> { new RequestItem(), new RequestItem() }
                }
            }
        };
    }

    private PriceAvailabilityParameter CreatePriceAvailabilityParameter(Guid? shipToId)
    {
        return new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { ShipToId = shipToId }
            }
        };
    }

    private void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }
}
