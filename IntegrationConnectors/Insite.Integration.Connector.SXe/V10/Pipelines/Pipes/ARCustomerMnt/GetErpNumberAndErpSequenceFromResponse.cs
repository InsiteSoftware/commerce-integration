namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.ARCustomerMntRequest;

using System.Text.RegularExpressions;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;

public sealed class GetErpNumberAndErpSequenceFromResponse
    : IPipe<ARCustomerMntParameter, ARCustomerMntResult>
{
    public int Order => 400;

    public ARCustomerMntResult Execute(
        IUnitOfWork unitOfWork,
        ARCustomerMntParameter parameter,
        ARCustomerMntResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(GetErpNumberAndErpSequenceFromResponse)} Started.");

        result.ErpNumber = GetErpNumber(result);
        result.ErpSequence = GetErpSequence(result);

        parameter.JobLogger?.Debug($"{nameof(GetErpNumberAndErpSequenceFromResponse)} Finished.");

        return result;
    }

    private static string GetErpNumber(ARCustomerMntResult getARCustomerMntResult)
    {
        var regex = new Regex("(?<=Customer #: )(.*?)(?=<|,|$)");
        var match = regex.Match(getARCustomerMntResult.ARCustomerMntResponse.ReturnData);

        return match.Value;
    }

    private static string GetErpSequence(ARCustomerMntResult getARCustomerMntResult)
    {
        var regex = new Regex("(?<=Ship To: )(.*?)(?=<|,|$)");
        var match = regex.Match(getARCustomerMntResult.ARCustomerMntResponse.ReturnData);

        return match.Value;
    }
}
