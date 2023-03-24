namespace Insite.Integration.Connector.SXe.Tests.V11.Pipelines.Pipes.ARCustomerMnt;

using System;
using System.Linq;

using NUnit.Framework;

using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.ARCustomerMntRequest;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.ARCustomerMnt;

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
    public void Execute_Should_Populate_InfieldModification()
    {
        var parameter = new ARCustomerMntParameter { Customer = billToCustomer };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            billToCustomer.CustomerType,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("custtype", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.CompanyName,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("name", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.Email,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("email", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.Phone,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("phoneno", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.Fax,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("faxphoneno", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.TermsCode,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("termstype", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.PriceCode,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("pricetype", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.CurrencyCode,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("currencyty", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.DefaultWarehouse.Name,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("whse", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.PrimarySalesperson.SalespersonNumber,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("slsrepout", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.Address1,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("addr1", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.Address2,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("addr2", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.City,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("city", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.State.Abbreviation,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("state", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.PostalCode,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("zipcd", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.ShipCode,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("shipviaty", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.BankCode,
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("bankno", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
        Assert.AreEqual(
            billToCustomer.CreditLimit.ToString(),
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("credlim", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
    }

    [Test]
    public void Execute_Should_Set_ErpNumber_And_CustomerSequence_With_BillTo_Customer()
    {
        var parameter = new ARCustomerMntParameter { Customer = billToCustomer };

        var result = this.RunExecute(parameter);

        Assert.IsTrue(
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications.All(
                o => o.Key1.Equals(string.Empty) && o.Key2.Equals(billToCustomer.CustomerSequence)
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
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications.All(
                o =>
                    o.Key1.Equals(billToCustomer.ErpNumber)
                    && o.Key2.Equals(shipToCustomer.CustomerSequence)
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
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("statustype", StringComparison.OrdinalIgnoreCase))
                .FieldValue
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
            result.ARCustomerMntRequest.Request.InFieldModificationCollection.InFieldModifications
                .First(o => o.FieldName.Equals("statustype", StringComparison.OrdinalIgnoreCase))
                .FieldValue
        );
    }

    private static Customer billToCustomer = Some.Customer()
        .WithIsBillTo(true)
        .WithErpNumber("erpnumber1")
        .WithCustomerSequence(string.Empty)
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
        return new ARCustomerMntResult
        {
            ARCustomerMntRequest = new ARCustomerMntRequest { Request = new Request() }
        };
    }
}
