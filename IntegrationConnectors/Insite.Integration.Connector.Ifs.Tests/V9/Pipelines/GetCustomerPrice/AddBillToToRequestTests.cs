namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.GetCustomerPrice;

using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

[TestFixture]
public class AddBillToToRequestTests
    : BaseForPipeTests<GetCustomerPriceParameter, GetCustomerPriceResult>
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
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Return_Result_When_BillToId_Is_Null()
    {
        Assert.DoesNotThrow(() => this.RunExecute());
    }

    [Test]
    public void Execute_Should_Throw_Exception_When_BillToId_Not_Found_In_Repository()
    {
        var parameter = new GetCustomerPriceParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { BillToId = Guid.NewGuid() }
            }
        };

        Assert.Throws<ArgumentException>(() => this.RunExecute(parameter));
    }

    [Test]
    public void Execute_Should_Return_Result_When_BillTo_ErpNumber_Is_Empty()
    {
        var customer = Some.Customer().Build();

        var parameter = new GetCustomerPriceParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { BillToId = customer.Id }
            }
        };

        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.That(result.CustomerPriceRequest.customerNo, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Set_Customer_Code_From_BillTo_PricingCustomer_ErpNumber()
    {
        var customer = Some.Customer()
            .WithPricingCustomer(Some.Customer().WithErpNumber("456"))
            .WithErpNumber("123")
            .Build();

        var parameter = new GetCustomerPriceParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { BillToId = customer.Id }
            }
        };

        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customer.PricingCustomer.ErpNumber, result.CustomerPriceRequest.customerNo);
    }

    [Test]
    public void Execute_Should_Set_Customer_Code_From_BillTo_ErpNumber()
    {
        var customer = Some.Customer().WithErpNumber("123").Build();

        var parameter = new GetCustomerPriceParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { BillToId = customer.Id }
            }
        };

        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customer.ErpNumber, result.CustomerPriceRequest.customerNo);
    }

    [Test]
    public void Execute_Should_Get_BillToId_From_GuestErpCustomerId_When_Pricing_Service_Parameter_BillToId_Is_Null()
    {
        var customer = Some.Customer().WithErpNumber("123").Build();

        var parameter = new GetCustomerPriceParameter
        {
            PricingServiceParameters = new List<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { BillToId = null }
            }
        };

        this.WhenGuestErpCustomerIdIs(customer.Id);
        this.WhenExists(customer);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customer.ErpNumber, result.CustomerPriceRequest.customerNo);
    }

    protected override GetCustomerPriceResult GetDefaultResult()
    {
        return new GetCustomerPriceResult { CustomerPriceRequest = new customerPriceRequest() };
    }

    protected void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }

    protected void WhenGuestErpCustomerIdIs(Guid guestErpCustomerId)
    {
        this.customerDefaultSettings.Setup(o => o.GuestErpCustomerId).Returns(guestErpCustomerId);
    }
}
