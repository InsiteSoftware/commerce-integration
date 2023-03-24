namespace Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetItemPrice;

using System;
using System.Linq;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Data.Entities;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public sealed class AddBillToToRequest : IPipe<GetItemPriceParameter, GetItemPriceResult>
{
    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddBillToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 300;

    public GetItemPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetItemPriceParameter parameter,
        GetItemPriceResult result
    )
    {
        var billToId = this.GetBillToId(parameter);
        if (billToId == null || billToId == Guid.Empty)
        {
            return result;
        }

        var billTo = unitOfWork.GetRepository<Customer>().Get(billToId.Value);
        if (billTo == null)
        {
            throw new ArgumentException($"Customer not found. Id: {billToId.Value}.");
        }

        var erpNumber = billTo.PricingCustomer?.ErpNumber ?? billTo.ErpNumber;
        if (string.IsNullOrEmpty(erpNumber))
        {
            // no default pricing customer and not yet submitted to erp
            return result;
        }

        result.GetItemPriceRequest.Request.CustomerCode = erpNumber;

        return result;
    }

    private Guid? GetBillToId(GetItemPriceParameter parameter)
    {
        return parameter.PricingServiceParameters.FirstOrDefault()?.BillToId
            ?? SiteContext.Current.BillTo?.Id
            ?? this.customerDefaultsSettings.GuestErpCustomerId;
    }
}
