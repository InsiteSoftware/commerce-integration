namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

public sealed class GetErpOrderNumberFromResponse
    : IPipe<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    public int Order => 1000;

    public SFOEOrderTotLoadV2Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV2Parameter parameter,
        SFOEOrderTotLoadV2Result result
    )
    {
        if (!parameter.IsOrderSubmit)
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(GetErpOrderNumberFromResponse)} Started.");

        result.ErpOrderNumber = GetErpOrderNum(result.SFOEOrderTotLoadV2Response);

        parameter.JobLogger?.Debug($"{nameof(GetErpOrderNumberFromResponse)} Finished.");

        return result;
    }

    private static string GetErpOrderNum(SFOEOrderTotLoadV2Response sfoeOrderTotLoadV2Response)
    {
        return
            sfoeOrderTotLoadV2Response?.arrayOutheader != null
            && sfoeOrderTotLoadV2Response.arrayOutheader.Any()
            ? sfoeOrderTotLoadV2Response.arrayOutheader[0].orderNumber
                + "-"
                + sfoeOrderTotLoadV2Response.arrayOutheader[0].orderSuffix
                    .ToString()
                    .PadLeft(2, '0')
            : string.Empty;
    }
}
