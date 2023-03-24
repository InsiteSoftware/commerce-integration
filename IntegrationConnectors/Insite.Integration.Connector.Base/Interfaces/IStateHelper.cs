namespace Insite.Integration.Connector.Base.Interfaces;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Data.Entities;

public interface IStateHelper : IDependency
{
    string GetBillToStateAbbreviation(IUnitOfWork unitOfWork, CustomerOrder customerOrder);

    string GetShipToStateAbbreviation(IUnitOfWork unitOfWork, CustomerOrder customerOrder);
}
