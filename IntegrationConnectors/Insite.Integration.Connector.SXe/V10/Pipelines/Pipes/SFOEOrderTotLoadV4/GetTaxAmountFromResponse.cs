namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;

public sealed class GetTaxAmountFromResponse
    : IPipe<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    public int Order => 1200;

    public SFOEOrderTotLoadV4Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV4Parameter parameter,
        SFOEOrderTotLoadV4Result result
    )
    {
        if (parameter.IsOrderSubmit)
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(GetTaxAmountFromResponse)} Started.");

        var taxAmount = GetTaxAmt(result.SFOEOrderTotLoadV4Response);
        result.TaxAmount = taxAmount ?? 0;
        result.TaxCalculated = taxAmount.HasValue;

        parameter.JobLogger?.Debug($"{nameof(GetTaxAmountFromResponse)} Finished.");

        return result;
    }

    private static decimal? GetTaxAmt(SFOEOrderTotLoadV4Response sfoeOrderTotLoadV4Response)
    {
        return sfoeOrderTotLoadV4Response?.Outouttotal?.FirstOrDefault()?.SalesTaxAmount;
    }
}
