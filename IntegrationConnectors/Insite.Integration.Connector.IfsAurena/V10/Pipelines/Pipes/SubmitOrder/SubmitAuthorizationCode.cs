namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitOrder;

using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class SubmitAuthorizationCode : IPipe<SubmitOrderParameter, SubmitOrderResult>
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    public SubmitAuthorizationCode(IPipeAssemblyFactory pipeAssemblyFactory)
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
    }

    public int Order => 900;

    public SubmitOrderResult Execute(
        IUnitOfWork unitOfWork,
        SubmitOrderParameter parameter,
        SubmitOrderResult result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(SubmitAuthorizationCode)} Started.");

        var submitAuthorizationCodeParameter = new SubmitAuthorizationCodeParameter
        {
            ErpOrderNumber = result.ErpOrderNumber,
            CustomerOrder = parameter.CustomerOrder,
            IntegrationConnection = parameter.IntegrationConnection,
            JobLogger = parameter.JobLogger
        };

        var submitAuthorizationCodeResult = this.pipeAssemblyFactory.ExecutePipeline(
            submitAuthorizationCodeParameter,
            new SubmitAuthorizationCodeResult()
        );
        if (submitAuthorizationCodeResult.ResultCode != ResultCode.Success)
        {
            return PipelineHelper.CopyResult(result, submitAuthorizationCodeResult);
        }

        parameter.JobLogger?.Debug($"{nameof(SubmitAuthorizationCode)} Finished.");

        return result;
    }
}
