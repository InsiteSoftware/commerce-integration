namespace Insite.Integration.Connector.Base.Tests.Helpers;

using FluentAssertions;
using Insite.Core.Interfaces.Data;
using Insite.Core.TestHelpers;
using Insite.Core.TestHelpers.Builders;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Base.Helpers;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CountryHelperTests
{
    private Mock<IUnitOfWork> unitOfWork;

    private Mock<ICountryRepository> countryRepository;

    private CountryHelper countryHelper;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        var dataProvider = container.GetMock<IDataProvider>();
        this.unitOfWork = container.GetMock<IUnitOfWork>();
        this.unitOfWork.Setup(o => o.DataProvider).Returns(dataProvider.Object);

        this.countryRepository = container.GetMock<ICountryRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<ICountryRepository>())
            .Returns(this.countryRepository.Object);

        this.countryHelper = container.Resolve<CountryHelper>();
    }

    [Test]
    public void GetBillToCountryAbbreviation_Should_Return_Country_Abbreviation_For_BTCountry()
    {
        var country = Some.Country().WithAbbreviation("CountryAbbreviation").Build();
        var customerOrder = Some.CustomerOrder().WithBTCountry("CountryName").Build();

        this.WhenGetCountryByNammeIs(customerOrder.BTCountry, country);

        var result = this.countryHelper.GetBillToCountryAbbreviation(
            this.unitOfWork.Object,
            customerOrder
        );

        result.Should().Be(country.Abbreviation);
    }

    [Test]
    public void GetBillToCountryAbbreviation_Should_Return_BTCountry_If_Country_For_BTCountry_Is_Null()
    {
        var customerOrder = Some.CustomerOrder().WithBTCountry("CountryName").Build();

        this.WhenGetCountryByNammeIs(customerOrder.BTCountry, null);

        var result = this.countryHelper.GetBillToCountryAbbreviation(
            this.unitOfWork.Object,
            customerOrder
        );

        result.Should().Be(customerOrder.BTCountry);
    }

    [TestCase("")]
    [TestCase(null)]
    public void GetBillToCountryAbbreviation_Should_Return_DefaultCountryAbbreviation_If_Country_For_BTCountry_Is_Null_And_BTCountry_Is_NullOrEmpty(
        string btCountry
    )
    {
        var customerOrder = Some.CustomerOrder().WithBTCountry(btCountry).Build();

        this.WhenGetCountryByNammeIs(customerOrder.BTCountry, null);

        var result = this.countryHelper.GetBillToCountryAbbreviation(
            this.unitOfWork.Object,
            customerOrder
        );

        result.Should().Be("US");
    }

    [Test]
    public void GetShipToCountryAbbreviation_Should_Return_Country_Abbreviation_For_STCountry()
    {
        var country = Some.Country().WithAbbreviation("CountryAbbreviation").Build();
        var customerOrder = Some.CustomerOrder().WithSTCountry("CountryName").Build();

        this.WhenGetCountryByNammeIs(customerOrder.STCountry, country);

        var result = this.countryHelper.GetShipToCountryAbbreviation(
            this.unitOfWork.Object,
            customerOrder
        );

        result.Should().Be(country.Abbreviation);
    }

    [Test]
    public void GetShipToCountryAbbreviation_Should_Return_STCountry_If_Country_For_STCountry_Is_Null()
    {
        var customerOrder = Some.CustomerOrder().WithSTCountry("CountryName").Build();

        this.WhenGetCountryByNammeIs(customerOrder.STCountry, null);

        var result = this.countryHelper.GetShipToCountryAbbreviation(
            this.unitOfWork.Object,
            customerOrder
        );

        result.Should().Be(customerOrder.STCountry);
    }

    [TestCase("")]
    [TestCase(null)]
    public void GetShipToCountryAbbreviation_Should_Return_DefaultCountryAbbreviation_If_Country_For_STCountry_Is_Null_And_STCountry_Is_NullOrEmpty(
        string stCountry
    )
    {
        var customerOrder = Some.CustomerOrder().WithSTCountry(stCountry).Build();

        this.WhenGetCountryByNammeIs(customerOrder.STCountry, null);

        var result = this.countryHelper.GetShipToCountryAbbreviation(
            this.unitOfWork.Object,
            customerOrder
        );

        result.Should().Be("US");
    }

    private void WhenGetCountryByNammeIs(string countryName, Country country)
    {
        this.countryRepository.Setup(o => o.GetCountryByName(countryName)).Returns(country);
    }
}
