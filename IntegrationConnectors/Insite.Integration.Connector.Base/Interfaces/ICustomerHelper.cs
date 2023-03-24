namespace Insite.Integration.Connector.Base.Interfaces;

using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Dependency;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Data.Entities;

public interface ICustomerHelper : IDependency
{
    Customer GetBillTo(IUnitOfWork unitOfWork, PricingServiceParameter pricingServiceParameter);

    Customer GetShipTo(IUnitOfWork unitOfWork, PricingServiceParameter pricingServiceParameter);

    Customer GetBillTo(IUnitOfWork unitOfWork, CustomerOrder customerOrder);

    Customer GetShipTo(IUnitOfWork unitOfWork, CustomerOrder customerOrder);
}
