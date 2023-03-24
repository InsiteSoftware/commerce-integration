namespace Insite.Integration.Connector.Base.Interfaces;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Data.Entities;

public interface ICountryHelper : IDependency
{
    string GetBillToCountryAbbreviation(IUnitOfWork unitOfWork, CustomerOrder customerOrder);

    string GetShipToCountryAbbreviation(IUnitOfWork unitOfWork, CustomerOrder customerOrder);
}
