namespace Insite.Integration.Connector.Acumatica.Tests.V18.Pipelines.CustomerPaymentMethod;

using System;
using System.Collections.Generic;
using System.Linq;
using Insite.Core.Interfaces.EnumTypes;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Data.Entities;
using Insite.Data.Repositories.Interfaces;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Parameters;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Pipes.CustomerPaymentMethod;
using Insite.Integration.Connector.Acumatica.V18.Pipelines.Results;
using Insite.Integration.Connector.Acumatica.V18.RestApi.Models.CustomerPaymentMethod;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AddPaymentInfoToRequestTests
    : BaseForPipeTests<CustomerPaymentMethodParameter, CustomerPaymentMethodResult>
{
    private Mock<ISystemListRepository> systemListRepository;

    public override Type PipeType => typeof(AddPaymentInfoToRequest);

    public override void SetUp()
    {
        this.systemListRepository = this.container.GetMock<ISystemListRepository>();
        this.unitOfWork
            .Setup(o => o.GetTypedRepository<ISystemListRepository>())
            .Returns(this.systemListRepository.Object);
    }

    [Test]
    public void Order_Is_200()
    {
        Assert.AreEqual(200, this.pipe.Order);
    }

    [Test]
    public void Execute_Should_Populate_Payment_Info_From_Credit_Card_Transaction()
    {
        var creditCardTransaction = Some.CreditCardTransaction()
            .WithCardType("VISA")
            .WithCustomerNumber("Erp123")
            .WithToken2("Token2")
            .Build();

        var systemListValues = new List<SystemListValue>
        {
            new SystemListValue { Description = "VISA", Name = "VISATOK" },
            new SystemListValue { Description = "DISCOVER", Name = "DISCOVERTOK" }
        };
        this.WhenGetActiveSystemListValuesIs(systemListValues);

        var parameter = this.GetDefaultParameter();
        parameter.CreditCardTransaction = creditCardTransaction;

        var result = this.RunExecute(parameter);

        Assert.AreEqual("VISATOK", result.CustomerPaymentMethodRequest.PaymentMethod);
        Assert.AreEqual(
            creditCardTransaction.CustomerNumber,
            result.CustomerPaymentMethodRequest.CustomerID
        );
        Assert.AreEqual(
            creditCardTransaction.Token2,
            result.CustomerPaymentMethodRequest.Details.First().Name
        );
    }

    protected override CustomerPaymentMethodResult GetDefaultResult()
    {
        return new CustomerPaymentMethodResult
        {
            CustomerPaymentMethodRequest = new CustomerPaymentMethod()
        };
    }

    protected void WhenGetActiveSystemListValuesIs(IList<SystemListValue> systemListValues)
    {
        this.systemListRepository
            .Setup(o => o.GetActiveSystemListValues(SystemListValueTypes.CreditCardTypeMapping))
            .Returns(systemListValues);
    }
}
