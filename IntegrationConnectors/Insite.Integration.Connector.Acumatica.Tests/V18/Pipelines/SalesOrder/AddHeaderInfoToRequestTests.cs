namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.SalesOrder;

using System;
using System.Collections.Generic;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.SalesOrder;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.SalesOrder;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AddHeaderInfoToRequestTests : BaseForPipeTests<SalesOrderParameter, SalesOrderResult>
{
    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    private Mock<ISystemListRepository> systemListRepository;

    public override Type PipeType => typeof(AddHeaderInfoToRequest);

    public override void SetUp()
    {
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();

        this.systemListRepository = this.container.GetMock<ISystemListRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<ISystemListRepository>())
            .Returns(this.systemListRepository.Object);

        this.WhenAcumaticaDisableAutomaticUpdatesIs(false);
    }

    [Test]
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_HeaderInfo()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.Currency().WithCurrencyCode("USD"))
            .WithOrderNumber("Web123")
            .With(Some.ShipVia().WithErpShipCode("FEDEX"))
            .WithNotes("TheNotes")
            .Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(
            customerOrder.Currency.CurrencyCode,
            result.SalesOrderRequest.BaseCurrencyID
        );
        Assert.AreEqual(customerOrder.Currency.CurrencyCode, result.SalesOrderRequest.CurrencyID);
        Assert.AreEqual(customerOrder.OrderNumber, result.SalesOrderRequest.ExternalRef);
        Assert.AreEqual("SO", result.SalesOrderRequest.OrderType);
        Assert.AreEqual(customerOrder.ShipVia.ErpShipCode, result.SalesOrderRequest.ShipVia);
        Assert.AreEqual(customerOrder.Notes, result.SalesOrderRequest.Note);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Execute_Should_Populate_DisableAutomaticDiscountUpdate_With_AcumaticaDisableAutomaticUpdates(
        bool acumaticaDisableAutomaticUpdates
    )
    {
        this.WhenAcumaticaDisableAutomaticUpdatesIs(acumaticaDisableAutomaticUpdates);

        var result = this.RunExecute();

        Assert.AreEqual(
            acumaticaDisableAutomaticUpdates,
            result.SalesOrderRequest.DisableAutomaticDiscountUpdate
        );
    }

    [Test]
    public void Execute_Should_Populate_PaymentMethod_From_SystemListValue_When_Payment_Is_CreditCard()
    {
        var customerOrder = Some.CustomerOrder()
            .With(Some.CreditCardTransaction().WithCardType("VISA"))
            .Build();

        var systemListValues = new List<SystemListValue>
        {
            new SystemListValue { Description = "VISA", Name = "VISATOK" },
            new SystemListValue { Description = "DISCOVER", Name = "DISCOVERTOK" }
        };
        this.WhenGetActiveSystemListValuesIs(systemListValues);

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual("VISATOK", result.SalesOrderRequest.PaymentMethod);
        Assert.IsNull(result.SalesOrderRequest.FinancialSettings);
    }

    [Test]
    public void Execute_Should_Populate_FinancialSettings_Terms_From_TermsCode_When_Payment_Is_Not_CreditCard()
    {
        var customerOrder = Some.CustomerOrder().WithTermsCode("NET30").Build();

        var parameter = this.GetDefaultParameter();
        parameter.CustomerOrder = customerOrder;

        var result = this.RunExecute(parameter);

        Assert.AreEqual(customerOrder.TermsCode, result.SalesOrderRequest.FinancialSettings.Terms);
        Assert.IsNull(result.SalesOrderRequest.PaymentMethod);
    }

    protected override SalesOrderParameter GetDefaultParameter()
    {
        return new SalesOrderParameter { CustomerOrder = new CustomerOrder() };
    }

    protected override SalesOrderResult GetDefaultResult()
    {
        return new SalesOrderResult { SalesOrderRequest = new SalesOrder() };
    }

    protected void WhenAcumaticaDisableAutomaticUpdatesIs(bool acumaticaDisableAutomaticUpdates)
    {
        this.integrationConnectorSettings
            .Setup(o => o.AcumaticaDisableAutomaticUpdates)
            .Returns(acumaticaDisableAutomaticUpdates);
    }

    protected void WhenGetActiveSystemListValuesIs(IList<SystemListValue> systemListValues)
    {
        this.systemListRepository
            .Setup(o => o.GetActiveSystemListValues(SystemListValueTypes.CreditCardTypeMapping))
            .Returns(systemListValues);
    }
}
