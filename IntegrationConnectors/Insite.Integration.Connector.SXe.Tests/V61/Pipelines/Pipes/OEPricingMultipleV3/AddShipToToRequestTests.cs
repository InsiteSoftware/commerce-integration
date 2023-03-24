namespace Insite.Integration.Connector.SXe.Tests.V6.Pipelines.Pipes.OEPricingMultipleV3;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.OEPricingMultipleV3;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.OEPricingMultipleV3;

[TestFixture]
public class AddShipToToRequestTests
    : BaseForPipeTests<OEPricingMultipleV3Parameter, OEPricingMultipleV3Result>
{
    private IList<Customer> customers;

    public override Type PipeType => typeof(AddShipToToRequest);

    public override void SetUp()
    {
        this.customers = new List<Customer>();
        this.fakeUnitOfWork.SetupEntityList(this.customers);
    }

    [Test]
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Not_Get_ShipTo_When_No_Pricing_Service_Parameters()
    {
        var parameter = new OEPricingMultipleV3Parameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>()
        };

        var result = this.RunExecute(parameter);

        Assert.That(result.OEPricingMultipleV3Request.shipTo, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Not_Get_ShipTo_When_ShipToId_Is_Null()
    {
        var parameter = this.CreateGetOEPricingMultipleV3Parameter(null);

        var result = this.RunExecute(parameter);

        Assert.That(result.OEPricingMultipleV3Request.shipTo, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Throw_Exception_When_ShipTo_Not_Found()
    {
        var shipTo = Some.Customer().WithId(Guid.NewGuid()).Build();

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(shipTo.Id);

        Assert.Throws<ArgumentException>(() => this.RunExecute(parameter));
    }

    [Test]
    public void Execute_Should_Not_Get_ShipTo_When_PricingCustomer_ErpSequence_Is_Empty_String()
    {
        var shipTo = Some.Customer()
            .WithId(Guid.NewGuid())
            .WithErpSequence("123")
            .WithPricingCustomer(Some.Customer().WithErpSequence(string.Empty))
            .Build();

        this.WhenExists(shipTo);

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(shipTo.Id);

        var result = this.RunExecute(parameter);

        Assert.That(result.OEPricingMultipleV3Request.shipTo, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Not_Get_ShipTo_When_ShipTo_ErpSequence_Is_Empty_String()
    {
        var shipTo = Some.Customer().WithId(Guid.NewGuid()).WithErpSequence(string.Empty).Build();

        this.WhenExists(shipTo);

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(shipTo.Id);

        var result = this.RunExecute(parameter);

        Assert.That(result.OEPricingMultipleV3Request.shipTo, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Get_ShipTo()
    {
        var shipTo = Some.Customer().WithId(Guid.NewGuid()).WithErpSequence("123").Build();

        this.WhenExists(shipTo);

        var parameter = this.CreateGetOEPricingMultipleV3Parameter(shipTo.Id);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(shipTo.ErpSequence, result.OEPricingMultipleV3Request.shipTo);
    }

    protected OEPricingMultipleV3Parameter CreateGetOEPricingMultipleV3Parameter(Guid? shipToId)
    {
        return new OEPricingMultipleV3Parameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { ShipToId = shipToId }
            }
        };
    }

    protected override OEPricingMultipleV3Result GetDefaultResult()
    {
        return new OEPricingMultipleV3Result
        {
            OEPricingMultipleV3Request = new OEPricingMultipleV3Request()
        };
    }

    protected void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }
}
