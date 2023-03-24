namespace Insite.Integration.Connector.SXe.Tests.V61.Pipelines.Pipes.ARCustomerMnt;

using System;
using System.Linq;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.ARCustomerMntRequest;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.ARCustomerMnt;

[TestFixture]
public class AddCustomerToRequestTests
    : BaseForPipeTests<ARCustomerMntParameter, ARCustomerMntResult>
{
    public override Type PipeType => typeof(AddCustomerToRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_arrayFieldModification()
    {
        var parameter = new ARCustomerMntParameter { Customer = billToCustomer };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            billToCustomer.CustomerType,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("custtype", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.CompanyName,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("name", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.Email,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("email", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.Phone,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("phoneno", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.Fax,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("faxphoneno", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.TermsCode,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("termstype", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.PriceCode,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("pricetype", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.CurrencyCode,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("currencyty", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.DefaultWarehouse.Name,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("whse", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.PrimarySalesperson.SalespersonNumber,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("slsrepout", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.Address1,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("addr1", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.Address2,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("addr2", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.City,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("city", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.State.Abbreviation,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("state", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.PostalCode,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("zipcd", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.ShipCode,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("shipviaty", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.BankCode,
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("bankno", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
        Assert.AreEqual(
            billToCustomer.CreditLimit.ToString(),
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("credlim", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
    }

    [Test]
    public void Execute_Should_Set_ErpNumber_And_CustomerSequence_With_BillTo_Customer()
    {
        var parameter = new ARCustomerMntParameter { Customer = billToCustomer };

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.ARCustomerMntRequest.arrayFieldModification.All(
                o => o.key1.Equals(string.Empty) && o.key2.Equals(billToCustomer.CustomerSequence)
            )
        );
    }

    [Test]
    public void Execute_Should_Set_ErpNumber_And_CustomerSequence_With_ShipTo_Customer()
    {
        shipToCustomer.Parent = billToCustomer;

        var parameter = new ARCustomerMntParameter { Customer = shipToCustomer };

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.ARCustomerMntRequest.arrayFieldModification.All(
                o =>
                    o.key1.Equals(billToCustomer.ErpNumber)
                    && o.key2.Equals(shipToCustomer.CustomerSequence)
            )
        );
    }

    [Test]
    public void Execute_Should_Set_StatusType_To_Active_When_Customer_IsActive_True()
    {
        var customer = Some.Customer().WithIsActive(true);

        var parameter = new ARCustomerMntParameter { Customer = customer };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "Active",
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("statustype", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
    }

    [Test]
    public void Execute_Should_Set_StatusType_To_InActive_When_Customer_IsActive_False()
    {
        var customer = Some.Customer().WithIsActive(false);

        var parameter = new ARCustomerMntParameter { Customer = customer };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            "InActive",
            result.ARCustomerMntRequest.arrayFieldModification
                .First(o => o.fieldName.Equals("statustype", StringComparison.OrdinalIgnoreCase))
                .fieldValue
        );
    }

    private static Customer billToCustomer = Some.Customer()
        .WithIsBillTo(true)
        .WithErpNumber(string.Empty)
        .WithCustomerSequence("customersequence2")
        .WithCustomerType("customertype")
        .WithCompanyName("Insite Software")
        .WithEmail("test@test.com")
        .WithPhone("5555555555")
        .WithFax("6666666666")
        .WithTermsCode("Net30")
        .WithPriceCode("pricecode")
        .WithCurrencyCode("USD")
        .WithDefaultWarehouse(Some.Warehouse().WithName("warehousename"))
        .WithPrimarySalesperson(Some.Salesperson().WithSalespersonNumber("salespersonnumber"))
        .WithAddress1("110 5th St N")
        .WithAddress2("8th Floor")
        .WithCity("Minneapolis")
        .With(Some.State().WithAbbreviation("MN"))
        .WithPostalCode("55042")
        .WithShipCode("shipcode")
        .WithBankCode("bankcode")
        .WithCreditLimit(10)
        .Build();

    private static Customer shipToCustomer = Some.Customer()
        .WithIsBillTo(false)
        .WithErpNumber(string.Empty)
        .WithCustomerSequence("customersequence2")
        .WithCustomerType("customertype")
        .WithCompanyName("Insite Software")
        .WithEmail("test@test.com")
        .WithPhone("5555555555")
        .WithFax("6666666666")
        .WithTermsCode("Net30")
        .WithPriceCode("pricecode")
        .WithCurrencyCode("USD")
        .WithDefaultWarehouse(Some.Warehouse().WithName("warehousename"))
        .WithPrimarySalesperson(Some.Salesperson().WithSalespersonNumber("salespersonnumber"))
        .WithAddress1("110 5th St N")
        .WithAddress2("8th Floor")
        .WithCity("Minneapolis")
        .With(Some.State().WithAbbreviation("MN"))
        .WithPostalCode("55042")
        .WithShipCode("shipcode")
        .WithBankCode("bankcode")
        .WithCreditLimit(10)
        .Build();

    protected override ARCustomerMntResult GetDefaultResult()
    {
        return new ARCustomerMntResult { ARCustomerMntRequest = new ARCustomerMntRequest() };
    }
}
