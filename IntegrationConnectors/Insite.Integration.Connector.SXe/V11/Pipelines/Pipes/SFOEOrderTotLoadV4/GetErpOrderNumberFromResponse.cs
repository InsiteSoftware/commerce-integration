namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System.Linq;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;
using Insite.Integration.Connector.SXe.V11.IonApi.Models.SFOEOrderTotLoadV4;

public sealed class GetErpOrderNumberFromResponse
    : IPipe<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    public int Order => 1100;

    public SFOEOrderTotLoadV4Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV4Parameter parameter,
        SFOEOrderTotLoadV4Result result
    )
    {
        if (!parameter.IsOrderSubmit)
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(GetErpOrderNumberFromResponse)} Started.");

        result.ErpOrderNumber = GetErpOrderNum(result.SFOEOrderTotLoadV4Response);

        parameter.JobLogger?.Debug($"{nameof(GetErpOrderNumberFromResponse)} Finished.");

        return result;
    }

    private static string GetErpOrderNum(SFOEOrderTotLoadV4Response sfoeOrderTotLoadV4Response)
    {
        return
            sfoeOrderTotLoadV4Response?.Response?.OrdLoadHdrDataCollection?.OrdLoadHdrDatas != null
            && sfoeOrderTotLoadV4Response.Response.OrdLoadHdrDataCollection.OrdLoadHdrDatas.Any()
            ? sfoeOrderTotLoadV4Response.Response.OrdLoadHdrDataCollection.OrdLoadHdrDatas
                .First()
                .Orderno
                + "-"
                + sfoeOrderTotLoadV4Response.Response.OrdLoadHdrDataCollection.OrdLoadHdrDatas
                    .First()
                    .Ordersuf.ToString()
                    .PadLeft(2, '0')
            : string.Empty;
    }
}
