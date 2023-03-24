namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.ARCustomerMntRequest;

using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.ARCustomerMnt;

public sealed class AddCustomerToRequest : IPipe<ARCustomerMntParameter, ARCustomerMntResult>
{
    public int Order => 200;

    public ARCustomerMntResult Execute(
        IUnitOfWork unitOfWork,
        ARCustomerMntParameter parameter,
        ARCustomerMntResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddCustomerToRequest)} Started.");

        var erpNumber = GetErpNumber(parameter.Customer);

        result.ARCustomerMntRequest.arrayFieldModification =
            new List<ARCustomerMntinputFieldModification>
            {
                GetARCustomerMntinputFieldModification(
                    0,
                    "custtype",
                    parameter.Customer.CustomerType,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    1,
                    "name",
                    parameter.Customer.CompanyName,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    2,
                    "email",
                    parameter.Customer.Email,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    3,
                    "phoneno",
                    parameter.Customer.Phone,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    4,
                    "faxphoneno",
                    parameter.Customer.Fax,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    5,
                    "termstype",
                    parameter.Customer.TermsCode,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    6,
                    "pricetype",
                    parameter.Customer.PriceCode,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    7,
                    "currencyty",
                    parameter.Customer.CurrencyCode,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    8,
                    "whse",
                    parameter.Customer.DefaultWarehouse?.Name,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    9,
                    "slsrepout",
                    parameter.Customer.PrimarySalesperson?.SalespersonNumber,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    10,
                    "addr1",
                    parameter.Customer.Address1,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    11,
                    "addr2",
                    parameter.Customer.Address2,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    12,
                    "city",
                    parameter.Customer.City,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    13,
                    "state",
                    parameter.Customer.State?.Abbreviation,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    14,
                    "zipcd",
                    parameter.Customer.PostalCode,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    15,
                    "shipviaty",
                    parameter.Customer.ShipCode,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    16,
                    "bankno",
                    parameter.Customer.BankCode,
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    17,
                    "credlim",
                    parameter.Customer.CreditLimit.ToString(),
                    erpNumber,
                    parameter.Customer.CustomerSequence
                ),
                GetARCustomerMntinputFieldModification(
                    18,
                    "statustype",
                    parameter.Customer.IsActive ? "Active" : "InActive",
                    erpNumber,
                    parameter.Customer.CustomerSequence
                )
            };

        parameter.JobLogger?.Debug($"{nameof(AddCustomerToRequest)} Finished.");

        return result;
    }

    private static string GetErpNumber(Customer customer)
    {
        return customer.IsBillTo ? string.Empty : customer.Parent?.ErpNumber ?? string.Empty;
    }

    private static ARCustomerMntinputFieldModification GetARCustomerMntinputFieldModification(
        int sequenceNumber,
        string fieldName,
        string fieldValue,
        string erpNumber,
        string erpSequence
    )
    {
        return new ARCustomerMntinputFieldModification
        {
            sequenceNumber = sequenceNumber,
            setNumber = 1,
            updateMode = "add",
            key1 = erpNumber,
            key2 = erpSequence,
            fieldName = fieldName,
            fieldValue = fieldValue
        };
    }
}
