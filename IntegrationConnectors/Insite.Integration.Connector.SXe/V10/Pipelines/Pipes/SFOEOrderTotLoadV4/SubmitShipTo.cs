namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Core.Services;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;

public sealed class SubmitShipTo : IPipe<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    private readonly IPipeAssemblyFactory pipeAssemblyFactory;

    private readonly StorefrontUserPermissionsSettings storefrontUserPermissionsSettings;

    public SubmitShipTo(
        IPipeAssemblyFactory pipeAssemblyFactory,
        StorefrontUserPermissionsSettings storefrontUserPermissionsSettings
    )
    {
        this.pipeAssemblyFactory = pipeAssemblyFactory;
        this.storefrontUserPermissionsSettings = storefrontUserPermissionsSettings;
    }

    public int Order => 200;

    public SFOEOrderTotLoadV4Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV4Parameter parameter,
        SFOEOrderTotLoadV4Result result
    )
    {
        if (
            !this.storefrontUserPermissionsSettings.AllowCreateAccount
            || !this.storefrontUserPermissionsSettings.AllowCreateNewShipToAddress
            || parameter.CustomerOrder.FulfillmentMethod.EqualsIgnoreCase(
                FulfillmentMethod.PickUp.ToString()
            )
            || parameter.CustomerOrder.ShipTo.IsDropShip
            || parameter.CustomerOrder.ShipTo.IsGuest
            || parameter.CustomerOrder.ShipTo == parameter.CustomerOrder.Customer
            || !parameter.CustomerOrder.ShipTo.ErpSequence.IsBlank()
            || parameter.CustomerOrder.ShipTo.Address1.IsBlank()
            || parameter.CustomerOrder.ShipTo.City.IsBlank()
        )
        {
            return result;
        }

        parameter.JobLogger?.Debug($"{nameof(SubmitShipTo)} Started.");

        var arCustomerMntParameter = new ARCustomerMntParameter
        {
            Customer = parameter.CustomerOrder.ShipTo,
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
            parameter.CustomerOrder.ShipTo.ErpNumber = arCustomerMntResult.ErpNumber;
            parameter.CustomerOrder.ShipTo.ErpSequence = arCustomerMntResult.ErpSequence;
        }

        parameter.JobLogger?.Debug($"{nameof(SubmitShipTo)} Finished.");

        return result;
    }
}
