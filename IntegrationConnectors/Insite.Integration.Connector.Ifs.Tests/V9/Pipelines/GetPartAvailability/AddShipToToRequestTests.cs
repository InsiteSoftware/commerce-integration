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
public class AddShipToToRequestTests
    : BaseForPipeTests<GetPartAvailabilityParameter, GetPartAvailabilityResult>
{
    private IList<Customer> customers;

    private Mock<CustomerDefaultsSettings> customerDefaultSettings;

    public override Type PipeType => typeof(AddShipToToRequest);

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
    public void Execute_Should_Return_Result_When_ShipToId_Is_Null()
    {
        Assert.DoesNotThrow(() => this.RunExecute());
    }

    [Test]
    public void Execute_Should_Get_ShipTo_From_SiteContext_ShipTo()
    {
        var customer = Some.Customer().WithErpSequence("123").Build();

        this.WhenSiteContextShipToIs(customer);

        var result = this.RunExecute();

        Assert.AreEqual(customer.ErpSequence, result.PartAvailabilityRequest.addressId);
    }

    [Test]
    public void Execute_Should_Set_Empty_ShipTo_When_SiteContext_ShipTo_ErpSequence_Is_Blank_And_Guest_CustomerId_Is_Empty_Guid()
    {
        this.WhenGuestErpCustomerIdIs(Guid.Empty);

        var result = this.RunExecute();

        Assert.That(result.PartAvailabilityRequest.addressId, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Set_Empty_ShipTo_When_SiteContext_ShipTo_ErpNumber_Is_Blank_And_Guest_CustomerId_Is_Not_Found_In_Repository()
    {
        var customer = Some.Customer().WithErpSequence("123").Build();

        this.WhenGuestErpCustomerIdIs(customer.Id);

        var result = this.RunExecute();

        Assert.That(result.PartAvailabilityRequest.addressId, Is.Null.Or.Empty);
    }

    [Test]
    public void Execute_Should_Get_ShipToId_From_GuestErpCustomerId_When_SiteContext_ShipTo_ErpNumber_Is_Blank()
    {
        var customer = Some.Customer().WithErpSequence("123").Build();

        this.WhenGuestErpCustomerIdIs(customer.Id);
        this.WhenExists(customer);

        var result = this.RunExecute();

        Assert.AreEqual(customer.ErpSequence, result.PartAvailabilityRequest.addressId);
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
