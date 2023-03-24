namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.ARCustomerMntRequest;

using System.Collections.Generic;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.ARCustomerMnt;

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

        result.ARCustomerMntRequest.InfieldModification = new List<InfieldModification>
        {
            GetInfieldModification(
                0,
                "custtype",
                parameter.Customer.CustomerType,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                1,
                "name",
                parameter.Customer.CompanyName,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                2,
                "email",
                parameter.Customer.Email,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                3,
                "phoneno",
                parameter.Customer.Phone,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                4,
                "faxphoneno",
                parameter.Customer.Fax,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                5,
                "termstype",
                parameter.Customer.TermsCode,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                6,
                "pricetype",
                parameter.Customer.PriceCode,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                7,
                "currencyty",
                parameter.Customer.CurrencyCode,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                8,
                "whse",
                parameter.Customer.DefaultWarehouse?.Name,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                9,
                "slsrepout",
                parameter.Customer.PrimarySalesperson?.SalespersonNumber,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                10,
                "addr1",
                parameter.Customer.Address1,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                11,
                "addr2",
                parameter.Customer.Address2,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                12,
                "city",
                parameter.Customer.City,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                13,
                "state",
                parameter.Customer.State?.Abbreviation,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                14,
                "zipcd",
                parameter.Customer.PostalCode,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                15,
                "shipviaty",
                parameter.Customer.ShipCode,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                16,
                "bankno",
                parameter.Customer.BankCode,
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
                17,
                "credlim",
                parameter.Customer.CreditLimit.ToString(),
                erpNumber,
                parameter.Customer.CustomerSequence
            ),
            GetInfieldModification(
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

    private static InfieldModification GetInfieldModification(
        int sequenceNumber,
        string fieldName,
        string fieldValue,
        string erpNumber,
        string erpSequence
    )
    {
        return new InfieldModification
        {
            SequenceNumber = sequenceNumber,
            SetNumber = 1,
            UpdateMode = "add",
            Key1 = erpNumber,
            Key2 = erpSequence,
            FieldName = fieldName,
            FieldValue = fieldValue
        };
    }
}
