namespace Insite.Integration.Connector.Ifs.Tests.V9.Pipelines.GetPartAvailability;

using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.WebServices.Models.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetPartAvailability;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

[TestFixture]
public class AddBillToToRequestTests
    : BaseForPipeTests<GetPartAvailabilityParameter, GetPartAvailabilityResult>
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
    public void Execute_Should_Get_BillTo_From_SiteContext_BillTo()
    {
        var customer = Some.Customer().WithErpNumber("123").Build();

        this.WhenSiteContextBillToIs(customer);

        var result = this.RunExecute();

        Assert.AreEqual(customer.ErpNumber, result.PartAvailabilityRequest.customerNo);
    }

    [Test]
    public void Execute_Should_Set_Empty_BillTo_When_SiteContext_BillTo_ErpNumber_Is_Blank_And_Guest_CustomerId_Is_Empty_Guid()
    {
        this.WhenGuestErpCustomerIdIs(Guid.Empty);

        var result = this.RunExecute();

        Assert.That(result.PartAvailabilityRequest.customerNo, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Set_Empty_BillTo_When_SiteContext_BillTo_ErpNumber_Is_Blank_And_Guest_CustomerId_Is_Not_Found_In_Repository()
    {
        var customer = Some.Customer().WithErpNumber("123").Build();

        this.WhenGuestErpCustomerIdIs(customer.Id);

        var result = this.RunExecute();

        Assert.That(result.PartAvailabilityRequest.customerNo, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Get_BillToId_From_GuestErpCustomerId_When_SiteContext_BillTo_ErpNumber_Is_Blank()
    {
        var customer = Some.Customer().WithErpNumber("123").Build();

        this.WhenGuestErpCustomerIdIs(customer.Id);
        this.WhenExists(customer);

        var result = this.RunExecute();

        Assert.AreEqual(customer.ErpNumber, result.PartAvailabilityRequest.customerNo);
    }

    protected override GetPartAvailabilityResult GetDefaultResult()
    {
        return new GetPartAvailabilityResult
        {
            PartAvailabilityRequest = new partAvailabilityRequest()
        };
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
