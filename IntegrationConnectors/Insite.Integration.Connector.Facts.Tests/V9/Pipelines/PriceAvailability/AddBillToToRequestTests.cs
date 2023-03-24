namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.PriceAvailability;

using System;
using System.Collections.Generic;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Facts.V9.Api.Models.PriceAvailability;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.PriceAvailability;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AddBillToToRequestTests
    : BaseForPipeTests<PriceAvailabilityParameter, PriceAvailabilityResult>
{
    private IList<Customer> customers;

    private Mock<CustomerDefaultsSettings> customerDefaultSettings;

    public override Type PipeType => typeof(AddBillToToRequest);

    public override void SetUp()
    {
        this.customers = new List<Customer>();
        this.fakeUnitOfWork.SetupEntityList(this.customers);

        this.customerDefaultSettings = this.container.GetMock<CustomerDefaultsSettings>();
        this.WhenGuestErpCustomerIdIs(Guid.Empty);
    }

    [Test]
    public void Order_Is_400()
    {
        Assert.AreEqual(400, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Return_Result_When_BillToId_Is_Null()
    {
        Assert.DoesNotThrow(() => this.RunExecute());
    }

    [Test]
    public void Execute_Should_Throw_Exception_When_BillToId_Not_Found_In_Repository()
    {
        var parameter = this.CreatePriceAvailabilityParameter(Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => this.RunExecute(parameter));
    }

    [Test]
    public void Execute_Should_Return_Result_When_BillTo_ErpNumber_Is_Empty()
    {
        var customer = Some.Customer().Build();

        var parameter = this.CreatePriceAvailabilityParameter(customer.Id);

        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.That(result.PriceAvailabilityRequest.Request.CustomerNumber, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Set_Customer_Code_From_BillTo_PricingCustomer_ErpNumber()
    {
        var customer = Some.Customer()
            .WithPricingCustomer(Some.Customer().WithErpNumber("456"))
            .WithErpNumber("123")
            .Build();

        var parameter = this.CreatePriceAvailabilityParameter(customer.Id);

        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customer.PricingCustomer.ErpNumber,
            result.PriceAvailabilityRequest.Request.CustomerNumber
        );
    }

    [Test]
    public void Execute_Should_Set_Customer_Code_From_BillTo_ErpNumber()
    {
        var customer = Some.Customer().WithErpNumber("123").Build();

        var parameter = this.CreatePriceAvailabilityParameter(customer.Id);

        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customer.ErpNumber, result.PriceAvailabilityRequest.Request.CustomerNumber);
    }

    [Test]
    public void Execute_Should_Get_BillToId_From_SiteContext_BillToId_When_Pricing_Service_Parameter_BillToId_Is_Null()
    {
        var customer = Some.Customer().WithErpNumber("123").Build();

        var parameter = this.CreatePriceAvailabilityParameter(null);

        this.WhenSiteContextBillToIs(customer);
        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customer.ErpNumber, result.PriceAvailabilityRequest.Request.CustomerNumber);
    }

    [Test]
    public void Execute_Should_Get_BillToId_From_GuestErpCustomerId_When_Pricing_Service_Parameter_BillToId_Is_Null_And_SiteContext_BillToId_Is_Null()
    {
        var customer = Some.Customer().WithErpNumber("123").Build();

        var parameter = this.CreatePriceAvailabilityParameter(null);

        this.WhenSiteContextBillToIs(null);
        this.WhenGuestErpCustomerIdIs(customer.Id);
        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customer.ErpNumber, result.PriceAvailabilityRequest.Request.CustomerNumber);
    }

    protected override PriceAvailabilityResult GetDefaultResult()
    {
        return new PriceAvailabilityResult
        {
            PriceAvailabilityRequest = new PriceAvailabilityRequest { Request = new Request() }
        };
    }

    private PriceAvailabilityParameter CreatePriceAvailabilityParameter(Guid? billToId)
    {
        return new PriceAvailabilityParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { BillToId = billToId }
            }
        };
    }

    private void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }

    private void WhenGuestErpCustomerIdIs(Guid guestErpCustomerId)
    {
        this.customerDefaultSettings.Setup(o => o.GuestErpCustomerId).Returns(guestErpCustomerId);
    }
}
