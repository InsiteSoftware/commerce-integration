namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.LineItemPriceAndAvailability;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Moq;
using NUnit.Framework;

using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.LineItemPriceAndAvailability;

[TestFixture]
public class AddRequestIdToRequestTests
    : BaseForPipeTests<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    private const string DefaultCompanyNumber = "1";

    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    private IList<Customer> customers;

    public override Type PipeType => typeof(AddRequestIdToRequest);

    public override void SetUp()
    {
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();

        this.customers = new List<Customer>();
        this.fakeUnitOfWork.SetupEntityList(this.customers);

        this.WhenAPlusCompanyIs(string.Empty);
    }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Get_CompanyNumber_From_IntegrationConnectorSettings_APlusCompany()
    {
        this.WhenAPlusCompanyIs("2,3");

        var result = this.RunExecute();
        var expectedRequestId = this.GetRequestId("2", string.Empty);

        Assert.AreEqual(expectedRequestId, result.LineItemPriceAndAvailabilityRequest.RequestID);
    }

    [Test]
    public void Execute_Should_Get_Default_CompanyNumber_When_IntegrationConnectorSettings_APlusCompany_Is_Not_Populated()
    {
        var result = this.RunExecute();
        var expectedRequestId = this.GetRequestId(DefaultCompanyNumber, string.Empty);

        Assert.AreEqual(expectedRequestId, result.LineItemPriceAndAvailabilityRequest.RequestID);
    }

    [Test]
    public void Execute_Should_Not_Get_BillTo_When_PricingServiceParameters_BillToId_Is_Null_And_SiteContext_BillTo_Is_Null()
    {
        this.WhenSiteContextBillToIs(null);

        var result = this.RunExecute();
        var expectedRequestId = this.GetRequestId(DefaultCompanyNumber, string.Empty);

        Assert.AreEqual(expectedRequestId, result.LineItemPriceAndAvailabilityRequest.RequestID);
    }

    [Test]
    public void Execute_Should_Throw_Exception_When_BillTo_Not_Found()
    {
        var billTo = Some.Customer().WithId(Guid.NewGuid()).Build();

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(billTo.Id);

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

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(billTo.Id);

        var result = this.RunExecute(parameter);
        var expectedRequestId = this.GetRequestId(DefaultCompanyNumber, string.Empty);

        Assert.AreEqual(expectedRequestId, result.LineItemPriceAndAvailabilityRequest.RequestID);
    }

    [Test]
    public void Execute_Should_Not_Get_BillTo_When_BillTo_ErpNumber_Is_Empty_String()
    {
        var billTo = Some.Customer().WithId(Guid.NewGuid()).WithErpNumber(string.Empty).Build();

        this.WhenExists(billTo);

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(billTo.Id);

        var result = this.RunExecute(parameter);
        var expectedRequestId = this.GetRequestId(DefaultCompanyNumber, string.Empty);

        Assert.AreEqual(expectedRequestId, result.LineItemPriceAndAvailabilityRequest.RequestID);
    }

    [Test]
    public void Execute_Should_Get_BillTo_PricingCustomer()
    {
        var billTo = Some.Customer()
            .WithId(Guid.NewGuid())
            .WithErpNumber("123")
            .WithPricingCustomer(Some.Customer().WithErpNumber("456"))
            .Build();

        this.WhenExists(billTo);

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(billTo.Id);

        var result = this.RunExecute(parameter);

        var expectedRequestId = this.GetRequestId(
            DefaultCompanyNumber,
            billTo.PricingCustomer.ErpNumber
        );

        Assert.AreEqual(expectedRequestId, result.LineItemPriceAndAvailabilityRequest.RequestID);
    }

    [Test]
    public void Execute_Should_Get_BillTo_From_PricingServiceParameters()
    {
        var billTo = Some.Customer().WithId(Guid.NewGuid()).WithErpNumber("123").Build();

        this.WhenExists(billTo);

        var parameter = this.CreateGetLineItemPriceAndAvailabilityParameter(billTo.Id);

        var result = this.RunExecute(parameter);

        var expectedRequestId = this.GetRequestId(DefaultCompanyNumber, billTo.ErpNumber);

        Assert.AreEqual(expectedRequestId, result.LineItemPriceAndAvailabilityRequest.RequestID);
    }

    [Test]
    public void Execute_Should_Get_BillTo_From_SiteContext()
    {
        var billTo = Some.Customer().WithId(Guid.NewGuid()).WithErpNumber("123").Build();

        this.WhenExists(billTo);
        this.WhenSiteContextBillToIs(billTo);

        var result = this.RunExecute();

        var expectedRequestId = this.GetRequestId(DefaultCompanyNumber, billTo.ErpNumber);

        Assert.AreEqual(expectedRequestId, result.LineItemPriceAndAvailabilityRequest.RequestID);
    }

    protected string GetRequestId(string companyNumber, string customerNumber)
    {
        return companyNumber.PadLeft(2, '0') + customerNumber.PadLeft(10, '0');
    }

    protected LineItemPriceAndAvailabilityParameter CreateGetLineItemPriceAndAvailabilityParameter(
        Guid? billToId
    )
    {
        return new LineItemPriceAndAvailabilityParameter
        {
            PricingServiceParameters = new Collection<PricingServiceParameter>
            {
                new PricingServiceParameter(Guid.Empty) { BillToId = billToId }
            }
        };
    }

    protected override LineItemPriceAndAvailabilityParameter GetDefaultParameter()
    {
        return this.CreateGetLineItemPriceAndAvailabilityParameter(null);
    }

    protected override LineItemPriceAndAvailabilityResult GetDefaultResult()
    {
        return new LineItemPriceAndAvailabilityResult
        {
            LineItemPriceAndAvailabilityRequest = new LineItemPriceAndAvailabilityRequest()
        };
    }

    private void WhenAPlusCompanyIs(string aPlusCompany)
    {
        this.integrationConnectorSettings.Setup(o => o.APlusCompany).Returns(aPlusCompany);
    }

    protected void WhenExists(Customer customer)
    {
        this.customers.Add(customer);
    }
}
