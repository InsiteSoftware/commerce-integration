namespace Insite.Integration.Connector.SXe.V11.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Integration.Connector.SXe.V11.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V11.Pipelines.Results;

public sealed class SubmitBillTo : IPipe<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    private readonly StorefrontUserPermissionsSettings storefrontUserPermissionsSettings;

    public SubmitBillTo(
        IPipeAssemblyFactory pipeAssemblyFactory,
        StorefrontUserPermissionsSettings storefrontUserPermissionsSettings
    )
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
        this.storefrontUserPermissionsSettings = storefrontUserPermissionsSettings;
    }

    public int Order => 100;

    public SFOEOrderTotLoadV4Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV4Parameter parameter,
        SFOEOrderTotLoadV4Result result
    )
    {
        if (
            !this.storefrontUserPermissionsSettings.AllowCreateAccount
            || parameter.CustomerOrder.Customer.IsGuest
            || !parameter.CustomerOrder.Customer.ErpNumber.IsBlank()
            || parameter.CustomerOrder.Customer.Address1.IsBlank()
            || parameter.CustomerOrder.Customer.City.IsBlank()
        )
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(SubmitBillTo)} Started.");

        var arCustomerMntParameter = new ARCustomerMntParameter
        {
            Customer = parameter.CustomerOrder.Customer,
            JobLogger = parameter.JobLogger
        };

        var arCustomerMntResult = this.pipeAssemblyFactory.ExecutePipeline(
            arCustomerMntParameter,
            new ARCustomerMntResult()
        );

        if (arCustomerMntResult.ResultCode != ResultCode.Success)
        {
            result.ResultCode = arCustomerMntResult.ResultCode;
            result.SubCode = arCustomerMntResult.SubCode;
            result.Messages = arCustomerMntResult.Messages;
        }
        else
        {
            parameter.CustomerOrder.Customer.ErpNumber = arCustomerMntResult.ErpNumber;
        }

        parameter.JobLogger?.Debug($"{nameof(SubmitBillTo)} Finished.");

        return result;
    }
}
