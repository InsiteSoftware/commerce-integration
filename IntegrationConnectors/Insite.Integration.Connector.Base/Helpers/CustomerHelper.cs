namespace Insite.Integration.Connector.Base.Helpers;

using System;
using Insite.Core.Context;
using Insite.Core.Interfaces.Data;
using Insite.Core.Interfaces.Plugins.Pricing;
using Insite.Core.SystemSetting.Groups.AccountManagement;
using Insite.Data.Entities;
using Insite.Integration.Connector.Base.Interfaces;

public class CustomerHelper : ICustomerHelper
{
    private readonly CustomerDefaultsSettings customerDefaultsSettings;

    public CustomerHelper(CustomerDefaultsSettings customerDefaultsSettings)
    {
        this.customerDefaultsSettings = customerDefaultsSettings;
    }

    public Customer GetBillTo(
        IUnitOfWork unitOfWork,
        PricingServiceParameter pricingServiceParameter
    )
    {
        var customerRepository = unitOfWork.GetRepository<Customer>();
        Customer billTo = null;

        if (pricingServiceParameter?.BillToId != null)
        {
            billTo = customerRepository.Get(pricingServiceParameter.BillToId.Value);
        }

        if (billTo == null)
        {
            billTo = SiteContext.Current.BillTo;
        }

        if (billTo == null && this.customerDefaultsSettings.GuestErpCustomerId != Guid.Empty)
        {
            billTo = customerRepository.Get(this.customerDefaultsSettings.GuestErpCustomerId);
        }

        return billTo?.PricingCustomer ?? billTo;
    }

    public Customer GetShipTo(
        IUnitOfWork unitOfWork,
        PricingServiceParameter pricingServiceParameter
    )
    {
        var customerRepository = unitOfWork.GetRepository<Customer>();
        Customer shipTo = null;

        if (pricingServiceParameter?.ShipToId != null)
        {
            shipTo = customerRepository.Get(pricingServiceParameter.ShipToId.Value);
        }

        if (shipTo == null)
        {
            shipTo = SiteContext.Current.ShipTo;
        }

        if (shipTo == null && this.customerDefaultsSettings.GuestErpCustomerId != Guid.Empty)
        {
            shipTo = customerRepository.Get(this.customerDefaultsSettings.GuestErpCustomerId);
        }

        return shipTo?.PricingCustomer ?? shipTo;
    }

    public Customer GetBillTo(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var billTo = customerOrder?.Customer;

        if (billTo == null && this.customerDefaultsSettings.GuestErpCustomerId != Guid.Empty)
        {
            billTo = unitOfWork
                .GetRepository<Customer>()
                .Get(this.customerDefaultsSettings.GuestErpCustomerId);
        }

        return billTo;
    }

    public Customer GetShipTo(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        var shipTo = customerOrder?.ShipTo;

        if (shipTo == null && this.customerDefaultsSettings.GuestErpCustomerId != Guid.Empty)
        {
            shipTo = unitOfWork
                .GetRepository<Customer>()
                .Get(this.customerDefaultsSettings.GuestErpCustomerId);
        }

        return shipTo;
    }
}
