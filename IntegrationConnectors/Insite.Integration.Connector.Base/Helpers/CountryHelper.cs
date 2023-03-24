namespace Insite.Integration.Connector.Base.Helpers;

using System;
using Insite.Core.Interfaces.Data;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Base.Interfaces;

public class CountryHelper : ICountryHelper
{
    public string GetBillToCountryAbbreviation(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        return GetCountryAbbreviation(unitOfWork, customerOrder.BTCountry);
    }

    public string GetShipToCountryAbbreviation(IUnitOfWork unitOfWork, CustomerOrder customerOrder)
    {
        return GetCountryAbbreviation(unitOfWork, customerOrder.STCountry);
    }

    private static string GetCountryAbbreviation(IUnitOfWork unitOfWork, string countryName)
    {
        var country = unitOfWork
            .GetTypedRepository<ICountryRepository>()
            .GetCountryByName(countryName);
        if (country != null)
        {
            return country.Abbreviation;
        }

        if (!countryName.IsBlank())
        {
            return countryName;
        }

        return "US";
    }
}
