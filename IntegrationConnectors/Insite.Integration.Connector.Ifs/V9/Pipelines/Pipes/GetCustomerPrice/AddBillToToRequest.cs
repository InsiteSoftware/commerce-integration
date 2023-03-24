namespace Insite.Integration.Connector.Ifs.V9.Pipelines.Pipes.GetCustomerPrice;

using System;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Data.Entities;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Ifs.V9.Pipelines.Results;

public sealed class AddBillToToRequest : IPipe<GetCustomerPriceParameter, GetCustomerPriceResult>
{
    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public AddBillToToRequest(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public int Order => 300;

    public GetCustomerPriceResult Execute(
        IUnitOfWork unitOfWork,
        GetCustomerPriceParameter parameter,
        GetCustomerPriceResult result
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

        result.CustomerPriceRequest.customerNo = erpNumber;

        return result;
    }

    private Guid? GetBillToId(GetCustomerPriceParameter parameter)
    {
        var billToId = parameter.PricingServiceParameters.FirstOrDefault()?.BillToId;
        if (billToId == null)
        {
            billToId = this.customerDefaultsSettings.GuestErpCustomerId;
        }

        return billToId;
    }
}
