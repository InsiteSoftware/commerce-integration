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
public class StateHelperTests
{
    private Mock<IUnitOfWork> unitOfWork;

    private Mock<IStateRepository> stateRepository;

    private StateHelper stateHelper;

    [SetUp]
    public void SetUp()
    {
        var container = new AutoMoqContainer();

        var dataProvider = container.GetMock<IDataProvider>();
        this.unitOfWork = container.GetMock<IUnitOfWork>();
        this.unitOfWork.Setup(o => o.DataProvider).Returns(dataProvider.Object);

        this.stateRepository = container.GetMock<IStateRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<IStateRepository>())
            .Returns(this.stateRepository.Object);

        this.stateHelper = container.Resolve<StateHelper>();
    }

    [Test]
    public void GetBillToStateAbbreviation_Should_Return_State_Abbreviation_For_BTState()
    {
        var state = Some.State().WithAbbreviation("StateAbbreviation").Build();
        var customerOrder = Some.CustomerOrder().WithBTState("StateName").Build();

        this.WhenGetStateByNammeIs(customerOrder.BTState, state);

        var result = this.stateHelper.GetBillToStateAbbreviation(
            this.unitOfWork.Object,
            customerOrder
        );

        result.Should().Be(state.Abbreviation);
    }

    [Test]
    public void GetBillToStateAbbreviation_Should_Return_BTState_If_State_For_BTState_Is_Null()
    {
        var customerOrder = Some.CustomerOrder().WithBTState("StateName").Build();

        this.WhenGetStateByNammeIs(customerOrder.BTState, null);

        var result = this.stateHelper.GetBillToStateAbbreviation(
            this.unitOfWork.Object,
            customerOrder
        );

        result.Should().Be(customerOrder.BTState);
    }

    [Test]
    public void GetShipToStateAbbreviation_Should_Return_State_Abbreviation_For_STState()
    {
        var state = Some.State().WithAbbreviation("StateAbbreviation").Build();
        var customerOrder = Some.CustomerOrder().WithSTState("StateName").Build();

        this.WhenGetStateByNammeIs(customerOrder.STState, state);

        var result = this.stateHelper.GetShipToStateAbbreviation(
            this.unitOfWork.Object,
            customerOrder
        );

        result.Should().Be(state.Abbreviation);
    }

    [Test]
    public void GetShipToStateAbbreviation_Should_Return_STState_If_State_For_STState_Is_Null()
    {
        var customerOrder = Some.CustomerOrder().WithSTState("StateName").Build();

        this.WhenGetStateByNammeIs(customerOrder.STState, null);

        var result = this.stateHelper.GetShipToStateAbbreviation(
            this.unitOfWork.Object,
            customerOrder
        );

        result.Should().Be(customerOrder.STState);
    }

    private void WhenGetStateByNammeIs(string stateName, State state)
    {
        this.stateRepository.Setup(o => o.GetStateByName(stateName)).Returns(state);
    }
}
