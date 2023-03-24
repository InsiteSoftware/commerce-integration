namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.OEPricingMultipleV3;

using System;
using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;

public sealed class AddBillToToRequest
    : IPipe<OEPricingMultipleV3Parameter, OEPricingMultipleV3Result>
{
    public int Order => 200;

    public OEPricingMultipleV3Result Execute(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV3Parameter parameter,
        OEPricingMultipleV3Result result
    )
    {
        result.OEPricingMultipleV3Request.customerNumber = GetCustomerNumber(unitOfWork, parameter);

        return result;
    }

    private static decimal GetCustomerNumber(
        IUnitOfWork unitOfWork,
        OEPricingMultipleV3Parameter parameter
    )
    {
        var billToId = parameter.PricingServiceParameters.FirstOrDefault()?.BillToId;
        if (billToId == null)
        {
            return default(decimal);
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
            return default(decimal);
        }

        if (!decimal.TryParse(erpNumber, out var customerNumber))
        {
            throw new ArgumentException("Customer ErpNumber must be in decimal format.");
        }

        return customerNumber;
    }
}
