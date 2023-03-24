namespace Insite.Integration.Connector.IfsAurena.V10.Pipelines.Pipes.SubmitAuthorizationCode;

using System;
using System.Linq;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Integration.Connector.IfsAurena.V10.OData.Models;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Parameters;
using Insite.Integration.Connector.IfsAurena.V10.Pipelines.Results;

public sealed class CreateRequests
    : IPipe<SubmitAuthorizationCodeParameter, SubmitAuthorizationCodeResult>
{
    public int Order => 100;

    public SubmitAuthorizationCodeResult Execute(
        IUnitOfWork unitOfWork,
        SubmitAuthorizationCodeParameter parameter,
        SubmitAuthorizationCodeResult result
    )
    {
        var transaction = parameter.CustomerOrder.CreditCardTransactions.FirstOrDefault(
            o => o.CreditCardNumber.IsNotBlank()
        );

        if (transaction == null)
        {
            result.ExitPipeline = true;
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(CreateRequests)} Started.");

        result.AuthorizationCodeRequest = new AuthorizationCode()
        {
            OrderNo = parameter.ErpOrderNumber,
            ReferenceId = transaction.PNRef,
            AuthCode = transaction.AuthCode
        };

        parameter.JobLogger?.Debug($"{nameof(CreateRequests)} Finished.");

        return result;
    }
}
