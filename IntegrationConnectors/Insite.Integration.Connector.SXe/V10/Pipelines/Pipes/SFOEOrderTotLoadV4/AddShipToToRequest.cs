namespace Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.Plugins.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;

public sealed class AddShipToToRequest
    : IPipe<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    public int Order => 800;

    public SFOEOrderTotLoadV4Result Execute(
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV4Parameter parameter,
        SFOEOrderTotLoadV4Result result
    )
    {
        parameter.JobLogger?.Debug($"{nameof(AddShipToToRequest)} Started.");

        var inInHeader3 = result.SFOEOrderTotLoadV4Request.Ininheader[0];
        result.SFOEOrderTotLoadV4Request.Ininheader[0] = UpdateInInHeader(
            inInHeader3,
            unitOfWork,
            parameter
        );

        parameter.JobLogger?.Debug($"{nameof(AddShipToToRequest)} Finished.");

        return result;
    }

    private static Ininheader3 UpdateInInHeader(
        Ininheader3 inInHeader3,
        IUnitOfWork unitOfWork,
        SFOEOrderTotLoadV4Parameter parameter
    )
    {
        inInHeader3.ShipToNumber = parameter.CustomerOrder.ShipTo?.ErpSequence ?? string.Empty;

        if (
            parameter.CustomerOrder.FulfillmentMethod.EqualsIgnoreCase(
                FulfillmentMethod.PickUp.ToString()
            )
            || parameter.CustomerOrder.ShipTo.IsDropShip
            || parameter.CustomerOrder.ShipTo.IsGuest
        )
        {
            inInHeader3.ShipToAddress1 = parameter.CustomerOrder.STAddress1;
            inInHeader3.ShipToAddress2 = parameter.CustomerOrder.STAddress2;
            inInHeader3.ShipToAddress3 = parameter.CustomerOrder.STAddress3;
            inInHeader3.ShipToAddress4 = parameter.CustomerOrder.STAddress4;
            inInHeader3.ShipToCity = parameter.CustomerOrder.STCity;
            inInHeader3.ShipToCountry = GetShipToCountry(unitOfWork, parameter.CustomerOrder);
            inInHeader3.ShipToContact = GetShipToContact(parameter.CustomerOrder);
            inInHeader3.ShipToName = GetShipToName(parameter.CustomerOrder);
            inInHeader3.ShipToPhone = parameter.CustomerOrder.STPhone;
            inInHeader3.ShipToState = GetShipToState(unitOfWork, parameter.CustomerOrder);
            inInHeader3.ShipToZipCode = parameter.CustomerOrder.STPostalCode;
        }

        return inInHeader3;
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
