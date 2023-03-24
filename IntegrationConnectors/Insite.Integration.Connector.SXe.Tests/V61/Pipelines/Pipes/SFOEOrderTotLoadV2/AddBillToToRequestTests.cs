namespace Insite.Integration.Connector.SXe.Tests.V6.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

public class AddBillToToRequestTests
    : BaseForPipeTests<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    private IList<Customer> customers;

    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    private Mock<CustomerDefaultsSettings> customerDefaultSettings;

    public override Type PipeType => typeof(AddBillToToRequest);

    public override void SetUp()
    {
        this.customers = new List<Customer>();
        this.fakeUnitOfWork.SetupEntityList(this.customers);

        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();
        this.customerDefaultSettings = this.container.GetMock<CustomerDefaultsSettings>();

        this.WhenSxeCompany(string.Empty);
    }

    [Test]
    public void Order_Is_600()
    {
        Assert.AreEqual(600, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_Company_Number_When_SxeCompany_Is_Delimited_List()
    {
        var sxeCompany = "3,4";

        this.WhenSxeCompany(sxeCompany);

        var result = this.RunExecute();

        Assert.AreEqual(
            "0003000000000000",
            result.SFOEOrderTotLoadV2Request.arrayInheader[0].customerID
        );
    }

    [Test]
    public void Execute_Should_Get_Company_Number_When_SxeCompany_Is_Single_Value()
    {
        var sxeCompany = "5";

        this.WhenSxeCompany(sxeCompany);

        var result = this.RunExecute();

        Assert.AreEqual(
            "0005000000000000",
            result.SFOEOrderTotLoadV2Request.arrayInheader[0].customerID
        );
    }

    [Test]
    public void Execute_Should_Get_Default_Company_Number_When_SxeCompany_Is_Empty()
    {
        var result = this.RunExecute();

        Assert.AreEqual(
            "0001000000000000",
            result.SFOEOrderTotLoadV2Request.arrayInheader[0].customerID
        );
    }

    [Test]
    public void Execute_Should_Get_Customer_Erp_Number_From_Customr_Order_Customer()
    {
        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder.Customer.ErpNumber = "123";

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "0001000000000123",
            result.SFOEOrderTotLoadV2Request.arrayInheader[0].customerID
        );
    }

    [Test]
    public void Execute_Should_Get_Customer_Erp_Number_From_Guest_Customer()
    {
        var guestCustomer = Some.Customer().WithErpNumber("456").Build();

        this.WhenExists(guestCustomer);
        this.WhenGuestErpCustomerId(guestCustomer.Id);

        var result = this.RunExecute();

        Assert.AreEqual(
            "0001000000000456",
            result.SFOEOrderTotLoadV2Request.arrayInheader[0].customerID
        );
    }

    protected override SFOEOrderTotLoadV2Parameter GetDefaultParameter()
    {
        return new SFOEOrderTotLoadV2Parameter
        {
            CustomerOrder = Some.CustomerOrder().With(Some.Customer())
        };
    }

    protected override SFOEOrderTotLoadV2Result GetDefaultResult()
    {
        return new SFOEOrderTotLoadV2Result
        {
            SFOEOrderTotLoadV2Request = new SFOEOrderTotLoadV2Request
            {
                arrayInheader = new List<SFOEOrderTotLoadV2inputInheader>
                {
                    new SFOEOrderTotLoadV2inputInheader()
                }
            }
        };
    }

    protected void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }

    protected void WhenSxeCompany(string sxeCompany)
    {
        this.integrationConnectorSettings.Setup(o => o.SXeCompany).Returns(sxeCompany);
    }

    protected void WhenGuestErpCustomerId(Guid guestErpCustomerId)
    {
        this.customerDefaultSettings.Setup(o => o.GuestErpCustomerId).Returns(guestErpCustomerId);
    }
}
