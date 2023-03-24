namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

public sealed class GetTaxAmountFromResponse
    : IPipe<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    public int Order => 1100;

    public SFOEOrderTotLoadV2Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV2Parameter parameter,
        SFOEOrderTotLoadV2Result result
    )
    {
        if (parameter.IsOrderSubmit)
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(GetTaxAmountFromResponse)} Started.");

        var taxAmount = GetTaxAmount(result.SFOEOrderTotLoadV2Response);
        result.TaxAmount = taxAmount ?? 0;
        result.TaxCalculated = taxAmount.HasValue;

        parameter.JobLogger?.Debug($"{nameof(GetTaxAmountFromResponse)} Finished.");

        return result;
    }

    private static decimal? GetTaxAmount(SFOEOrderTotLoadV2Response sfoeOrderTotLoadV2Response)
    {
        return sfoeOrderTotLoadV2Response?.arrayOuttotal?.FirstOrDefault()?.salesTaxAmount;
    }
}
