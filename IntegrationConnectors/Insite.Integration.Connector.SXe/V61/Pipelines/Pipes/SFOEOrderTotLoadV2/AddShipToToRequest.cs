namespace Insite.Integration.Connector.SXe.V61.Pipelines.Pipes.SFOEOrderTotLoadV2;

using System;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.SXe.V61.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V61.Pipelines.Results;
using Insite.Integration.Connector.SXe.V61.SXApi.Models.SFOEOrderTotLoadV2;

public sealed class AddShipToToRequest
    : IPipe<SFOEOrderTotLoadV2Parameter, SFOEOrderTotLoadV2Result>
{
    public int Order => 700;

    public SFOEOrderTotLoadV2Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV2Parameter parameter,
        SFOEOrderTotLoadV2Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddShipToToRequest)} Started.");

        var inInHeader1 = result.SFOEOrderTotLoadV2Request.arrayInheader[0];
        result.SFOEOrderTotLoadV2Request.arrayInheader[0] = UpdateInInHeader(
            inInHeader1,
            unitOfWork,
            parameter
        );

        parameter.JobLogger?.Debug($"{nameof(AddShipToToRequest)} Finished.");

        return result;
    }

    private static SFOEOrderTotLoadV2inputInheader UpdateInInHeader(
        SFOEOrderTotLoadV2inputInheader inInHeader1,
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV2Parameter parameter
    )
    {
        inInHeader1.shipToNumber = parameter.CustomerOrder.ShipTo?.ErpSequence ?? string.Empty;

        if (
            parameter.CustomerOrder.FulfillmentMethod.EqualsIgnoreCase(
                FulfillmentMethod.PickUp.ToString()
            )
            || parameter.CustomerOrder.ShipTo.IsDropShip
            || parameter.CustomerOrder.ShipTo.IsGuest
        )
        {
            inInHeader1.shipToAddress1 = parameter.CustomerOrder.STAddress1;
            inInHeader1.shipToAddress2 = parameter.CustomerOrder.STAddress2;
            inInHeader1.shipToAddress3 = parameter.CustomerOrder.STAddress3;
            inInHeader1.shipToAddress4 = parameter.CustomerOrder.STAddress4;
            inInHeader1.shipToCity = parameter.CustomerOrder.STCity;
            inInHeader1.shipToCountry = GetShipToCountry(unitOfWork, parameter.CustomerOrder);
            inInHeader1.shipToContact = GetShipToContact(parameter.CustomerOrder);
            inInHeader1.shipToName = GetShipToName(parameter.CustomerOrder);
            inInHeader1.shipToPhone = parameter.CustomerOrder.STPhone;
            inInHeader1.shipToState = GetShipToState(unitOfWork, parameter.CustomerOrder);
            inInHeader1.shipToZipCode = parameter.CustomerOrder.STPostalCode;
        }

        return inInHeader1;
    }

    private static string GetShipToContact(CustomerOrder customerOrder)
    {
        return $"{customerOrder.STFirstName} {customerOrder.STLastName}";
    }

    private static string GetShipToName(CustomerOrder customerOrder)
    {
        if (customerOrder.FulfillmentMethod.EqualsIgnoreCase(FulfillmentMethod.PickUp.ToString()))
        {
            var shipToName = $"Pick up at {customerOrder.STCompanyName}";
            return shipToName.Length <= 30 ? shipToName : shipToName.Substring(0, 30);
        }
        else
        {
            return customerOrder.STCompanyName;
        }
    }

    private static string GetShipToState(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var shipToState = unitOfWork
            .GetTypedRepository<IStateRepository>()
            .GetStateByName(customerOrder.STState);
        return shipToState != null ? shipToState.Abbreviation : customerOrder.STState;
    }

    private static string GetShipToCountry(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var shipToCountry = unitOfWork
            .GetTypedRepository<ICountryRepository>()
            .GetCountryByName(customerOrder.STCountry);
        return shipToCountry != null ? shipToCountry.Abbreviation : customerOrder.STCountry;
    }
}
