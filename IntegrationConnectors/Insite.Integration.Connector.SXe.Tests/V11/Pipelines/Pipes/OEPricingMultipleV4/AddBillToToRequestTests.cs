namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.OEPricingMultipleV4;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.OEPricingMultipleV4;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.OEPricingMultipleV4;

[TestFixture]
public class AddBillToToRequestTests
    : BaseForPipeTests<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    private IList<Customer> customers;

    public override Type PipeType => typeof(AddBillToToRequest);

    public override void SetUp()
    {
        this.customers = new List<Customer>();
        this.fakeUnitOfWork.SetupEntityList(this.customers);
    }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Not_Get_BillTo_When_No_Pricing_Service_Parameters()
    {
        var parameter = new OEPricingMultipleV4Parameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>()
        };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(0, result.OEPricingMultipleV4Request.Request.CustomerNumber);
    }

    [Test]
    public void Execute_Should_Not_Get_BillTo_When_BillToId_Is_Null()
    {
        var parameter = this.CreateGetOEPricingMultipleV4Parameter(null);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(0, result.OEPricingMultipleV4Request.Request.CustomerNumber);
    }

    [Test]
    public void Execute_Should_Throw_Exception_When_BillTo_Not_Found()
    {
        var billTo = Some.Customer().WithId(Guid.NewGuid()).Build();

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(billTo.Id);

        Assert.Throws<ArgumentException>(() => this.RunExecute(parameter));
    }

    [Test]
    public void Execute_Should_Not_Get_BillTo_When_PricingCustomer_ErpNumber_Is_Empty_String()
    {
        var billTo = Some.Customer()
            .WithId(Guid.NewGuid())
            .WithErpNumber("123")
            .WithPricingCustomer(Some.Customer().WithErpNumber(string.Empty))
            .Build();

        this.WhenExists(billTo);

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(billTo.Id);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(0, result.OEPricingMultipleV4Request.Request.CustomerNumber);
    }

    [Test]
    public void Execute_Should_Not_Get_BillTo_When_BillTo_ErpNumber_Is_Empty_String()
    {
        var billTo = Some.Customer().WithId(Guid.NewGuid()).WithErpNumber(string.Empty).Build();

        this.WhenExists(billTo);

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(billTo.Id);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(0, result.OEPricingMultipleV4Request.Request.CustomerNumber);
    }

    [Test]
    public void Execute_Should_Throw_Exception_When_BillTo_ErpNumber_Is_Not_Decimal()
    {
        var billTo = Some.Customer().WithId(Guid.NewGuid()).WithErpNumber("ABC").Build();

        this.WhenExists(billTo);

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(billTo.Id);

        Assert.Throws<ArgumentException>(() => this.RunExecute(parameter));
    }

    [Test]
    public void Execute_Should_Get_BillTo()
    {
        var billTo = Some.Customer().WithId(Guid.NewGuid()).WithErpNumber("123").Build();

        this.WhenExists(billTo);

        var parameter = this.CreateGetOEPricingMultipleV4Parameter(billTo.Id);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            long.Parse(billTo.ErpNumber),
            result.OEPricingMultipleV4Request.Request.CustomerNumber
        );
    }

    protected OEPricingMultipleV4Parameter CreateGetOEPricingMultipleV4Parameter(Guid? billToId)
    {
        return new OEPricingMultipleV4Parameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { BillToId = billToId }
            }
        };
    }

    protected override OEPricingMultipleV4Result GetDefaultResult()
    {
        return new OEPricingMultipleV4Result
        {
            OEPricingMultipleV4Request = new OEPricingMultipleV4Request { Request = new Request() }
        };
    }

    protected void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }
}
