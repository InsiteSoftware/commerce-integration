namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.CreateOrder;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.CreateOrder;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class AddHeaderInfoToRequest : IPipe<CreateOrderParameter, CreateOrderResult>
{
    public int Order => 200;

    public CreateOrderResult Execute(
        IUnitOfWork unitOfWork,
        CreateOrderParameter parameter,
        CreateOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Started.");
        var specialCharacterRegex = new Regex("[!@#$%^&*()-+=/\\{}[]|:;\"'<>,.?~`;]");

        var orderHeader = new RequestOrderHeader
        {
            CarrierCode = parameter.CustomerOrder.ShipVia?.ErpShipCode ?? string.Empty,
            OrderSource = "WB",
            OrderType = "O",
            PONumber = parameter.CustomerOrder.CustomerPO,
            ReqShipDate = GetRequestedShipDate(parameter.CustomerOrder),
            WarehouseId = parameter.CustomerOrder.DefaultWarehouse?.Name ?? string.Empty,
            WebOrderID = parameter.IsOrderSubmit
                ? parameter.CustomerOrder.OrderNumber
                : string.Empty,
            WebTransactionType = parameter.IsOrderSubmit ? "LSF" : "TSF",
            WebUserID = specialCharacterRegex.Replace(parameter.CustomerOrder.PlacedByUserName, "_")
        };

        if (parameter.CustomerOrder.CreditCardTransactions.Any())
        {
            var creditCardTransaction = parameter.CustomerOrder.CreditCardTransactions.First();

            orderHeader.CCPONumber = parameter.CustomerOrder.CustomerPO;
            orderHeader.CCCreditCardNbr = creditCardTransaction.CreditCardNumber;
            orderHeader.CCCreditCardExp = creditCardTransaction.ExpirationDate;
            orderHeader.CCAuthorizationAmount = creditCardTransaction.Amount.ToString();
            orderHeader.CCZip = creditCardTransaction.AvsZip;
            orderHeader.CCPaymentType = TransformCardTypeToPayType(creditCardTransaction.CardType);
            orderHeader.CCCVV2 = creditCardTransaction.AuthCode;
        }

        result.CreateOrderRequest.Orders = new List<RequestOrder>
        {
            new RequestOrder { OrderHeader = orderHeader }
        };

        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Finished.");

        return result;
    }

    private static string TransformCardTypeToPayType(string cardType)
    {
        switch (cardType.ToLower())
        {
            case "mastercard":
                return "MC";
            case "americanexpress":
                return "am";
            case "discover":
                return "dc";
            case "visa":
                return "vi";
            default:
                return cardType;
        }
    }

    private static string GetRequestedShipDate(CustomerOrder customerOrder)
    {
        var requestedShipDate = customerOrder.FulfillmentMethod.EqualsIgnoreCase(
            FulfillmentMethod.PickUp.ToString()
        )
            ? customerOrder.RequestedPickupDate
            : customerOrder.RequestedDeliveryDate;

        return requestedShipDate?.ToString("yyyyMMdd") ?? string.Empty;
    }
}
