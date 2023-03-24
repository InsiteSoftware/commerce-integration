namespace Insite.Integration.Connector.Base.Helpers;

using Insite.Core.Interfaces.Data;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Base.Interfaces;

public class StateHelper : IStateHelper
{
    public string GetBillToStateAbbreviation(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        return GetStateAbbreviation(unitOfWork, customerOrder.BTState);
    }

    public string GetShipToStateAbbreviation(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        return GetStateAbbreviation(unitOfWork, customerOrder.STState);
    }

    private static string GetStateAbbreviation(IUnitOfWork unitOfWork, string stateName)
    {
        var state = unitOfWork.GetTypedRepository<IStateRepository>().GetStateByName(stateName);

        return state?.Abbreviation ?? stateName;
    }
}
