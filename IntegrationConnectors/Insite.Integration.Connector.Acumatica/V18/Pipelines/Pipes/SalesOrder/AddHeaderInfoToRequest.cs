namespace Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;

using System.Linq;
using Insite.Common.Providers;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;

public sealed class AddHeaderInfoToRequest : IPipe<SalesOrderParameter, SalesOrderResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddHeaderInfoToRequest(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 300;

    public SalesOrderResult Execute(
        IUnitOfWork unitOfWork,
        SalesOrderParameter parameter,
        SalesOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Started.");

        result.SalesOrderRequest.BaseCurrencyID = parameter.CustomerOrder.Currency?.CurrencyCode;
        result.SalesOrderRequest.CurrencyID = parameter.CustomerOrder.Currency?.CurrencyCode;
        result.SalesOrderRequest.DisableAutomaticDiscountUpdate =
            this.integrationConnectorSettings.AcumaticaDisableAutomaticUpdates;
        result.SalesOrderRequest.ExternalRef = parameter.CustomerOrder.OrderNumber;
        result.SalesOrderRequest.OrderType = "SO";
        result.SalesOrderRequest.ShipVia = parameter.CustomerOrder.ShipVia?.ErpShipCode;
        result.SalesOrderRequest.Note = parameter.CustomerOrder.Notes;

        var now = DateTimeProvider.Current.Now.DateTime;
        result.SalesOrderRequest.Date = now;
        result.SalesOrderRequest.EffectiveDate = now;
        result.SalesOrderRequest.RequestedOn = now;

        var creditCardTransation = parameter.CustomerOrder.CreditCardTransactions.FirstOrDefault();
        if (creditCardTransation != null)
        {
            result.SalesOrderRequest.PaymentMethod = GetPaymentMethod(
                unitOfWork,
                creditCardTransation
            );
        }
        else
        {
            result.SalesOrderRequest.FinancialSettings = new Financialsettings
            {
                Terms = parameter.CustomerOrder.TermsCode
            };
        }

        parameter.JobLogger?.Debug($"{nameof(AddHeaderInfoToRequest)} Finished.");

        return result;
    }

    private static string GetPaymentMethod(
        IUnitOfWork unitOfWork,
        CreditCardTransaction creditCardTransaction
    )
    {
        return unitOfWork
            .GetTypedRepository<ISystemListRepository>()
            .GetActiveSystemListValues(SystemListValueTypes.CreditCardTypeMapping)
            .FirstOrDefault(o => o.Description.Equals(creditCardTransaction.CardType))
            ?.Name;
    }
}
