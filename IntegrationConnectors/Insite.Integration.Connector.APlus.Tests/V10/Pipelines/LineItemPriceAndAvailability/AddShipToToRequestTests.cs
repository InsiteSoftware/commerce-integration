namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.LineItemPriceAndAvailability;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class AddShipToToRequestTests
    : BaseForPipeTests<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
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
    public void Execute_Should_Not_Set_ShipToNumber_When_PricingServiceParameters_ShipToId_Is_Null_And_SiteContext_ShipTo_Is_Null()
    {
        this.WhenSiteContextShipToIs(null);

        var paramter = this.CreateGetLineItemPriceAndAvailabilityParameter(null);

        var result = this.RunExecute(paramter);

        Assert.IsTrue(
            result.LineItemPriceAndAvailabilityRequest.Items.All(o => o.ShipToNumber.IsBlank())
        );
    }

    [Test]
    public void Execute_Should_Throw_Exception_When_ShipTo_Not_Found()
    {
        var paramter = this.CreateGetLineItemPriceAndAvailabilityParameter(Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => this.RunExecute(paramter));
    }

    [Test]
    public void Execute_Should_Get_ShipTo_PricingCustomer()
    {
        var shipTo = Some.Customer()
            .WithId(Guid.NewGuid())
            .WithErpSequence("123")
            .WithPricingCustomer(Some.Customer().WithErpSequence("456"))
            .Build();

        this.WhenExists(shipTo);

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(shipTo.Id);

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.LineItemPriceAndAvailabilityRequest.Items.All(
                o => o.ShipToNumber == shipTo.PricingCustomer.ErpSequence
            )
        );
    }

    [Test]
    public void Execute_Should_Get_ShipTo_From_PricingServiceParameters()
    {
        var shipTo = Some.Customer().WithId(Guid.NewGuid()).WithErpSequence("123").Build();

        this.WhenExists(shipTo);

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(shipTo.Id);

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.LineItemPriceAndAvailabilityRequest.Items.All(
                o => o.ShipToNumber == shipTo.ErpSequence
            )
        );
    }

    [Test]
    public void Execute_Should_Get_ShipTo_From_SiteContext()
    {
        var shipTo = Some.Customer().WithId(Guid.NewGuid()).WithErpSequence("123").Build();

        this.WhenExists(shipTo);
        this.WhenSiteContextShipToIs(shipTo);

        var result = this.RunExecute();

        Assert.IsTrue(
            result.LineItemPriceAndAvailabilityRequest.Items.All(
                o => o.ShipToNumber == shipTo.ErpSequence
            )
        );
    }

    protected LineItemPriceAndAvailabilityParameter CreateGetLineItemPriceAndAvailabilityParameter(
        Guid? shipToId
    )
    {
        return new LineItemPriceAndAvailabilityParameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { ShipToId = shipToId }
            }
        };
    }

    protected override LineItemPriceAndAvailabilityResult GetDefaultResult()
    {
        return new LineItemPriceAndAvailabilityResult
        {
            LineItemPriceAndAvailabilityRequest = new LineItemPriceAndAvailabilityRequest
            {
                Items = new List<RequestItem> { new RequestItem() }
            }
        };
    }

    protected void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }
}
