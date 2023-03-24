namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.OEPricingMultipleV4;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;

public sealed class AddBillToToRequest
    : IPipe<OEPricingMultipleV4Parameter, OEPricingMultipleV4Result>
{
    public int Order => 200;

    public OEPricingMultipleV4Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV4Parameter parameter,
        OEPricingMultipleV4Result result
    )
    {
        result.OEPricingMultipleV4Request.Request.CustomerNumber = GetCustomerNumber(
            unitOfWork,
            parameter
        );

        return result;
    }

    private static long GetCustomerNumber(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV4Parameter parameter
    )
    {
        var billToId = parameter.PricingServiceParameters.FirstOrDefault()?.BillToId;
        if (billToId == null)
        {
            return default(long);
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
            return default(long);
        }

        if (!long.TryParse(erpNumber, out var customerNumber))
        {
            throw new ArgumentException("Customer ErpNumber must be in integer format.");
        }

        return customerNumber;
    }
}
