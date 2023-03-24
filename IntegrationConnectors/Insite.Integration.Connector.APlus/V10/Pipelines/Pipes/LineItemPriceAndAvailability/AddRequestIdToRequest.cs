namespace Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.LineItemPriceAndAvailability;

using System;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Data.Entities;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

public sealed class AddRequestIdToRequest
    : IPipe<LineItemPriceAndAvailabilityParameter, LineItemPriceAndAvailabilityResult>
{
    private readonly IntegrationConnectorSettings integrationConnectorSettings;

    public AddRequestIdToRequest(IntegrationConnectorSettings integrationConnectorSettings)
    {
        this.integrationConnectorSettings = integrationConnectorSettings;
    }

    public int Order => 200;

    public LineItemPriceAndAvailabilityResult Execute(
        IUnitOfWork unitOfWork,
        LineItemPriceAndAvailabilityParameter parameter,
        LineItemPriceAndAvailabilityResult result
    )
    {
        var companyNumber = this.GetCompanyNumber();
        var customerNumber = this.GetCustomerNumber(unitOfWork, parameter);

        var requestId = companyNumber.PadLeft(2, '0') + customerNumber.PadLeft(10, '0');

        result.LineItemPriceAndAvailabilityRequest.RequestID = requestId;

        return result;
    }

    private string GetCompanyNumber()
    {
        var companyNumbers = this.integrationConnectorSettings.APlusCompany.Split(
            new[] { ',', ';', '|' },
            StringSplitOptions.RemoveEmptyEntries
        );

        return companyNumbers.Any() ? companyNumbers[0] : "1";
    }

    private string GetCustomerNumber(
        IUnitOfWork unitOfWork,
        LineItemPriceAndAvailabilityParameter parameter
    )
    {
        var billToId = GetBillToId(parameter);
        if (billToId == null)
        {
            return string.Empty;
        }

        var billTo = unitOfWork.GetRepository<Customer>().Get(billToId.Value);
        if (billTo == null)
        {
            throw new ArgumentException($"No customer found with id {billToId.Value}.");
        }

        var erpNumber = billTo.PricingCustomer?.ErpNumber ?? billTo.ErpNumber;
        if (string.IsNullOrEmpty(erpNumber))
        {
            // no default pricing customer and not yet submitted to erp
            return string.Empty;
        }

        return erpNumber;
    }

    private static Guid? GetBillToId(LineItemPriceAndAvailabilityParameter parameter)
    {
        return parameter.PricingServiceParameters?.FirstOrDefault()?.BillToId
            ?? SiteContext.Current.BillTo?.Id;
    }
}
