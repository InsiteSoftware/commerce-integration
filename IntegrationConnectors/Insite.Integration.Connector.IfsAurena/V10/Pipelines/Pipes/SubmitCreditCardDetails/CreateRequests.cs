namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitCreditCardDetails;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Integration.Connector.Base.Interfaces;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class CreateRequests
    : IPipe<SubmitCreditCardDetailsParameter, SubmitCreditCardDetailsResult>
{
    private readonly ICustomerHelper customerHelper;

    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public CreateRequests(
        ICustomerHelper customerHelper,
        IntegrationConnectorSettings integrationConnectorSettings
    )
    {
        this.customerHelper = customerHelper;
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 100;

    public SubmitCreditCardDetailsResult Execute(
        IUnitOfWork unitOfWork,
        SubmitCreditCardDetailsParameter parameter,
        SubmitCreditCardDetailsResult result
    )
    {
        var transaction = parameter.CustomerOrder.CreditCardTransactions.FirstOrDefault(
            o => o.ExpirationDate.IsNotBlank()
        );

        if (transaction == null)
        {
            result.ExitPipeline = true;
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(CreateRequests)} Started.");

        var billTo = this.customerHelper.GetBillTo(unitOfWork, parameter.CustomerOrder);

        result.CreditCardDetailsRequest = new CreditCardDetails
        {
            OrderNo = parameter.ErpOrderNumber,
            DisplayCardNumber = transaction.Token1,
            SingleOccurrence = false,
            CreditExpMonth = $"Value{transaction.ExpirationDate.Split('/')[0]}",
            CreditExpYear = int.Parse(transaction.ExpirationDate.Split('/')[1]),
            CustomerNo = parameter.CustomerOrder.CustomerNumber,
            Currency = parameter.CustomerOrder.Currency.CurrencyCode,
            CardType = transaction.CardType,
            Company = this.integrationConnectorSettings.IfsAurenaCompany,
            FirstName = billTo.FirstName,
            LastName = billTo.LastName
        };

        parameter.JobLogger?.Debug($"{nameof(CreateRequests)} Finished.");

        return result;
    }
}
