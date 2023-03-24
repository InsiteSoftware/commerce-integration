namespace Insite.Integration.Connector.SXe.Tests.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;

using System;
using System.Collections.Generic;
using System.Linq;

using Moq;
using NUnit.Framework;

using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.SXe.V10.Pipelines.Parameters;
using Insite.Integration.Connector.SXe.V10.Pipelines.Pipes.SFOEOrderTotLoadV4;
using Insite.Integration.Connector.SXe.V10.Pipelines.Results;
using Insite.Integration.Connector.SXe.V10.SXApi.Models.SFOEOrderTotLoadV4;

[TestFixture]
public class AddShipToToRequestTests
    : BaseForPipeTests<SFOEOrderTotLoadV4Parameter, SFOEOrderTotLoadV4Result>
{
    private Mock<IStateRepository> stateRepository;

    private Mock<ICountryRepository> countryRepository;

    public override Type PipeType => typeof(AddShipToToRequest);

    public override void SetUp()
    {
        this.stateRepository = this.container.GetMock<IStateRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<IStateRepository>())
            .Returns(this.stateRepository.Object);

        this.countryRepository = this.container.GetMock<ICountryRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<ICountryRepository>())
            .Returns(this.countryRepository.Object);
    }

    [Test]
    public void Order_Is_800()
    {
        Assert.AreEqual(800, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_ShipTo_Number()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithErpSequence("ERP123"))
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.ShipTo.ErpSequence,
            result.SFOEOrderTotLoadV4Request.Ininheader.First().ShipToNumber
        );
    }

    [Test]
    public void Execute_Should_Populate_Ship_To_Properties_When_Customer_Order_FulfillmentMethod_Is_PickUp()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithSTAddress1("123 ABC ST")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.STAddress1,
            result.SFOEOrderTotLoadV4Request.Ininheader.First().ShipToAddress1
        );
    }

    [Test]
    public void Execute_Should_Populate_Ship_To_Properties_When_Customer_Order_Ship_To_Is_Drop_Ship()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithIsDropShip(true))
            .WithSTAddress1("123 ABC ST")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.STAddress1,
            result.SFOEOrderTotLoadV4Request.Ininheader.First().ShipToAddress1
        );
    }

    [Test]
    public void Execute_Should_Populate_Ship_To_Properties_When_Customer_Order_Ship_To_Is_Guest()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer().WithIsGuest(true))
            .WithSTAddress1("123 ABC ST")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.STAddress1,
            result.SFOEOrderTotLoadV4Request.Ininheader.First().ShipToAddress1
        );
    }

    [Test]
    public void Execute_Should_Not_Populate_Ship_To_Properties_When_Customer_Order_FulfillmentMethod_Is_Not_PickUp_And_Ship_To_Is_Not_Drop_Ship()
    {
        var customerOrder = Some.CustomerOrder()
            .WithShipTo(Some.Customer())
            .WithSTCompanyName("Insite")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.That(
            result.SFOEOrderTotLoadV4Request.Ininheader.First().ShipToName,
            Is.Null.Or.Empty
        );
    }

    [Test]
    public void Execute_Should_Populate_Ship_To_State_And_Country_From_State_And_Country_Entity_Abbreviation()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithSTState("Minnesota")
            .WithSTCountry("United States")
            .Build();

        var state = Some.State().WithAbbreviation("MN").Build();
        var country = Some.Country().WithAbbreviation("USA").Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        this.WhenGetStateByNameIs(customerOrder.STState, state);
        this.WhenGetCountryByNameIs(customerOrder.STCountry, country);

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            state.Abbreviation,
            result.SFOEOrderTotLoadV4Request.Ininheader[0].ShipToState
        );
        Assert.AreEqual(
            country.Abbreviation,
            result.SFOEOrderTotLoadV4Request.Ininheader[0].ShipToCountry
        );
    }

    [Test]
    public void Execute_Should_Get_Ship_To_Contact_From_Customer_Order()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithSTFirstName("John")
            .WithSTLastName("Denver")
            .Build();

        var parameter = new SFOEOrderTotLoadV4Parameter { CustomerOrder = customerOrder };

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            $"{customerOrder.STFirstName} {customerOrder.STLastName}",
            result.SFOEOrderTotLoadV4Request.Ininheader[0].ShipToContact
        );
    }

    [Test]
    public void Execute_Should_Populate_Custom_Ship_To_Name_When_Customer_Order_FulfillmentMethod_Is_PickUp()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithSTCompanyName("Insite")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            $"Pick up at {customerOrder.STCompanyName}",
            result.SFOEOrderTotLoadV4Request.Ininheader.First().ShipToName
        );
    }

    [Test]
    public void Execute_Should_Populate_Custom_Ship_To_Name_And_Truncate_To_Thirty_Characters_When_Customer_Order_FulfillmentMethod_Is_PickUp()
    {
        var customerOrder = Some.CustomerOrder()
            .WithFulfillmentMethod(FulfillmentMethod.PickUp.ToString())
            .WithSTCompanyName("Insite Software Solutions - Am I Over Thirty Characters Yet")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            $"Pick up at {customerOrder.STCompanyName}".Substring(0, 30),
            result.SFOEOrderTotLoadV4Request.Ininheader.First().ShipToName
        );
    }

    protected override SFOEOrderTotLoadV4Result GetDefaultResult()
    {
        return new SFOEOrderTotLoadV4Result
        {
            SFOEOrderTotLoadV4Request = new SFOEOrderTotLoadV4Request
            {
                Ininheader = new List<Ininheader3> { new Ininheader3() }
            }
        };
    }

    protected void WhenGetStateByNameIs(string name, State state)
    {
        this.stateRepository.Setup(o => o.GetStateByName(name)).Returns(state);
    }

    protected void WhenGetCountryByNameIs(string name, Country country)
    {
        this.countryRepository.Setup(o => o.GetCountryByName(name)).Returns(country);
    }
}
