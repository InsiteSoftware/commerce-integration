namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;
using Insite.PaymentGateway.Cenpos;

public sealed class AddInInFieldValuesToRequest
    : IPipe<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    private readonly CenposSettings cenposSettings;

    public AddInInFieldValuesToRequest(CenposSettings cenposSettings)
    {
        this.cenposSettings = cenposSettings;
    }

    public int Order => 600;

    public SFOEOrderTotLoadV4Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV4Parameter parameter,
        SFOEOrderTotLoadV4Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddInInFieldValuesToRequest)} Started.");

        result.SFOEOrderTotLoadV4Request.Ininfieldvalue = this.GetInInFieldValues(
            parameter.CustomerOrder
        );

        parameter.JobLogger?.Debug($"{nameof(AddInInFieldValuesToRequest)} Finished.");

        return result;
    }

    private List<Ininfieldvalue4> GetInInFieldValues(CustomerOrder customerOrder)
    {
        var inInFieldValue4s = new List<Ininfieldvalue4>();

        if (!customerOrder.CreditCardTransactions.Any())
        {
            return inInFieldValue4s;
        }

        inInFieldValue4s.Add(
            new Ininfieldvalue4
            {
                Level = "SFOEOrderTotLoadV4",
                LineNumber = 0,
                SequenceNumber = 0,
                FieldName = "AuthAmt",
                FieldValue = customerOrder.CreditCardTransactions.First().Amount.ToString()
            }
        );

        inInFieldValue4s.Add(
            new Ininfieldvalue4
            {
                Level = "SFOEOrderTotLoadV4",
                LineNumber = 0,
                SequenceNumber = 0,
                FieldName = "AuthNumber",
                FieldValue = customerOrder.CreditCardTransactions.First().AuthCode
            }
        );

        inInFieldValue4s.Add(
            new Ininfieldvalue4
            {
                Level = "SFOEOrderTotLoadV4",
                LineNumber = 0,
                SequenceNumber = 0,
                FieldName = "ProcPaymentType",
                FieldValue = TransformCardTypeToPayType(
                    customerOrder.CreditCardTransactions.First().CardType
                )
            }
        );

        inInFieldValue4s.Add(
            new Ininfieldvalue4
            {
                Level = "SFOEOrderTotLoadV4",
                LineNumber = 0,
                SequenceNumber = 0,
                FieldName = "PaymentType",
                FieldValue = "cenpos"
            }
        );

        inInFieldValue4s.Add(
            new Ininfieldvalue4
            {
                Level = "SFOEOrderTotLoadV4",
                LineNumber = 0,
                SequenceNumber = 0,
                FieldName = "ReferenceNumber",
                FieldValue = customerOrder.CreditCardTransactions.First().PNRef
            }
        );

        inInFieldValue4s.Add(
            new Ininfieldvalue4
            {
                Level = "SFOEOrderTotLoadV4",
                LineNumber = 0,
                SequenceNumber = 0,
                FieldName = "MerchantID",
                FieldValue = this.cenposSettings.MerchantId.ToString()
            }
        );

        inInFieldValue4s.Add(
            new Ininfieldvalue4
            {
                Level = "SFOEOrderTotLoadV4",
                LineNumber = 0,
                SequenceNumber = 0,
                FieldName = "CardNumber",
                FieldValue = customerOrder.CreditCardTransactions.First().CreditCardNumber
            }
        );

        inInFieldValue4s.Add(
            new Ininfieldvalue4
            {
                Level = "SFOEOrderTotLoadV4",
                LineNumber = 0,
                SequenceNumber = 0,
                FieldName = "Token",
                FieldValue = null
            }
        );

        return inInFieldValue4s;
    }

    private static string TransformCardTypeToPayType(string cardType)
    {
        switch (cardType.ToLower())
        {
            case "mastercard":
                return "MASTERCARD";
            case "americanexpress":
                return "AMEX";
            case "discover":
                return "DISCOVER";
            case "visa":
                return "VISA";
            default:
                return cardType;
        }
    }
}
