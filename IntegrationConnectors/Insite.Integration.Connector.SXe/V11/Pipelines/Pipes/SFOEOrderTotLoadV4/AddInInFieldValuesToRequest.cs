namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Collections.Generic;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFOEOrderTotLoadV4;
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

        result.SFOEOrderTotLoadV4Request.Request.InFieldValueCollection = new InFieldValueCollection
        {
            InFieldValues = this.GetInInFieldValues(parameter.CustomerOrder)
        };

        parameter.JobLogger?.Debug($"{nameof(AddInInFieldValuesToRequest)} Finished.");

        return result;
    }

    private List<InFieldValue> GetInInFieldValues(CustomerOrder customerOrder)
    {
        var inInFieldValue4s = new List<InFieldValue>();
        var ccTransaction = customerOrder.CreditCardTransactions.FirstOrDefault(
            o => !o.CardType.EqualsIgnoreCase("paypal")
        );

        if (ccTransaction == null)
        {
            return inInFieldValue4s;
        }

        inInFieldValue4s.Add(
            new InFieldValue
            {
                Level = "SFOEOrderTotLoadV4",
                Lineno = 0,
                Seqno = 0,
                Fieldname = "AuthAmt",
                Fieldvalue = ccTransaction.Amount.ToString()
            }
        );

        inInFieldValue4s.Add(
            new InFieldValue
            {
                Level = "SFOEOrderTotLoadV4",
                Lineno = 0,
                Seqno = 0,
                Fieldname = "AuthNumber",
                Fieldvalue = ccTransaction.AuthCode
            }
        );

        inInFieldValue4s.Add(
            new InFieldValue
            {
                Level = "SFOEOrderTotLoadV4",
                Lineno = 0,
                Seqno = 0,
                Fieldname = "ProcPaymentType",
                Fieldvalue = TransformCardTypeToPayType(ccTransaction.CardType)
            }
        );

        inInFieldValue4s.Add(
            new InFieldValue
            {
                Level = "SFOEOrderTotLoadV4",
                Lineno = 0,
                Seqno = 0,
                Fieldname = "PaymentType",
                Fieldvalue = "cenpos"
            }
        );

        inInFieldValue4s.Add(
            new InFieldValue
            {
                Level = "SFOEOrderTotLoadV4",
                Lineno = 0,
                Seqno = 0,
                Fieldname = "ReferenceNumber",
                Fieldvalue = ccTransaction.PNRef
            }
        );

        inInFieldValue4s.Add(
            new InFieldValue
            {
                Level = "SFOEOrderTotLoadV4",
                Lineno = 0,
                Seqno = 0,
                Fieldname = "MerchantID",
                Fieldvalue = this.cenposSettings.MerchantId.ToString()
            }
        );

        inInFieldValue4s.Add(
            new InFieldValue
            {
                Level = "SFOEOrderTotLoadV4",
                Lineno = 0,
                Seqno = 0,
                Fieldname = "CardNumber",
                Fieldvalue = ccTransaction.CreditCardNumber
            }
        );

        inInFieldValue4s.Add(
            new InFieldValue
            {
                Level = "SFOEOrderTotLoadV4",
                Lineno = 0,
                Seqno = 0,
                Fieldname = "Token",
                Fieldvalue = null
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
