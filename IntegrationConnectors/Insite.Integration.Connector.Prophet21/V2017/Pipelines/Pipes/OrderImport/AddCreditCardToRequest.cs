namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.OrderImport;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.OrderImport;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddCreditCardToRequest : IPipe<OrderImportParameter, OrderImportResult>
{
    [Obsolete("Use SystemListValueTypes.CreditCardTypeMapping instead.")]
    public const string CreditCardTypeMapping = SystemListValueTypes.CreditCardTypeMapping;

    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddCreditCardToRequest(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 700;

    public OrderImportResult Execute(
        IUnitOfWork unitOfWork,
        OrderImportParameter parameter,
        OrderImportResult result
    )
    {
        if (!parameter.CustomerOrder.CreditCardTransactions.Any())
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(AddCreditCardToRequest)} Started.");

        var cardTypesDictionary = unitOfWork
            .GetTypedRepository<ISystemListRepository>()
            .GetTableAsNoTracking()
            .Where(o => o.Name == SystemListValueTypes.CreditCardTypeMapping)
            .SelectMany(o => o.Values)
            .Select(o => new { o.Description, o.Name })
            .ToList()
            .ToDictionary(o => o.Description.ToUpper(), o => o.Name);

        result.OrderImportRequest.Request.CreditCard = this.GetRequestCreditCard(
            unitOfWork,
            parameter.CustomerOrder,
            parameter.CustomerOrder.CreditCardTransactions.First(o => o.Result == "0"),
            cardTypesDictionary
        );

        parameter.JobLogger?.Debug($"{nameof(AddCreditCardToRequest)} Finished.");

        return result;
    }

    private RequestCreditCard GetRequestCreditCard(
        IUnitOfWork unitOfWork,
        CustomerOrder customerOrder,
        CreditCardTransaction creditCardTransaction,
        IReadOnlyDictionary<string, string> cardTypesDictionary
    )
    {
        return new RequestCreditCard
        {
            CardType = GetMatchingCardType(creditCardTransaction.CardType, cardTypesDictionary),
            AuthorizationCode = this.integrationConnectorSettings.Prophet21SendAuthCodeInOrderSubmit
                ? creditCardTransaction.AuthCode
                : null,
            CardNumber = MaskCardNumber(creditCardTransaction.CreditCardNumber),
            ExpirationMonth = GetParsedDatePart(creditCardTransaction.ExpirationDate, "MM"),
            ExpirationYear = GetParsedDatePart(creditCardTransaction.ExpirationDate, "yy"),
            ElementPaymentAccountID = creditCardTransaction.Token2,
            ElementTransactionID = creditCardTransaction.PNRef,
            CardHolder = GetRequestCardHolder(unitOfWork, customerOrder, creditCardTransaction),
            ChargeAmount = creditCardTransaction.Amount
        };
    }

    private static string GetMatchingCardType(
        string transactionCardType,
        IReadOnlyDictionary<string, string> cardTypesDictionary
    )
    {
        var isCardTypeMatched = cardTypesDictionary.TryGetValue(
            transactionCardType.ToUpper(),
            out var matchingCardType
        );
        return isCardTypeMatched ? matchingCardType : string.Empty;
    }

    private static string MaskCardNumber(string creditCardNumber)
    {
        return creditCardNumber.Length > 4
            ? creditCardNumber
                .Substring(creditCardNumber.Length - 4)
                .PadLeft(creditCardNumber.Length, '*')
            : string.Empty;
    }

    private static string GetParsedDatePart(string expirationDate, string dateFormat)
    {
        var isExpirationDateParsed = DateTime.TryParseExact(
            expirationDate,
            "MM/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var expirationDateTime
        );
        return isExpirationDateParsed ? expirationDateTime.ToString(dateFormat) : string.Empty;
    }

    private static RequestCardHolder GetRequestCardHolder(
        IUnitOfWork unitOfWork,
        CustomerOrder customerOrder,
        CreditCardTransaction creditCardTransaction
    )
    {
        var index = creditCardTransaction.Name.IndexOf(" ");

        return new RequestCardHolder
        {
            FirstName =
                index < 0
                    ? creditCardTransaction.Name
                    : creditCardTransaction.Name.Substring(0, index),
            LastName = index < 0 ? string.Empty : creditCardTransaction.Name.Substring(index + 1),
            Address = GetRequestAddress(unitOfWork, customerOrder, creditCardTransaction)
        };
    }

    private static RequestAddress GetRequestAddress(
        IUnitOfWork unitOfWork,
        CustomerOrder customerOrder,
        CreditCardTransaction creditCardTransaction
    )
    {
        var addressTokens = creditCardTransaction?.AvsAddr.Split('|');

        return new RequestAddress
        {
            Address1 =
                addressTokens?.Length == 5 ? addressTokens[0].Trim() : customerOrder.BTAddress1,
            Address2 =
                addressTokens?.Length == 5 ? addressTokens[1].Trim() : customerOrder.BTAddress2,
            City = addressTokens?.Length == 5 ? addressTokens[2].Trim() : customerOrder.BTCity,
            State = GetStateByName(
                unitOfWork,
                addressTokens?.Length == 5 ? addressTokens[3].Trim() : customerOrder.BTState
            ),
            Country = GetCountryByName(
                unitOfWork,
                addressTokens?.Length == 5 ? addressTokens[4].Trim() : customerOrder.BTCountry
            ),
            Zip =
                addressTokens?.Length == 5
                    ? creditCardTransaction.AvsZip
                    : customerOrder.BTPostalCode
        };
    }

    private static string GetStateByName(IUnitOfWork unitOfWork, string stateName)
    {
        var shipToState = unitOfWork
            .GetTypedRepository<IStateRepository>()
            .GetStateByName(stateName);
        return shipToState != null ? shipToState.Abbreviation : stateName;
    }

    private static string GetCountryByName(IUnitOfWork unitOfWork, string countryName)
    {
        var shipToCountry = unitOfWork
            .GetTypedRepository<ICountryRepository>()
            .GetCountryByName(countryName);
        return shipToCountry != null ? shipToCountry.Abbreviation : countryName;
    }
}
