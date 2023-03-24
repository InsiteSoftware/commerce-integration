namespace Insite.Integration.Connector.Facts.Tests.V9.Pipelines.CustomerSummary;

using System;
using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Facts.V9.Api.Models.CustomerSummary;
using Insite.Integration.Connector.Facts.V9.Pipelines.Parameters;
using Insite.Integration.Connector.Facts.V9.Pipelines.Results;
using Insite.Integration.Connector.Facts.V9.Pipelines.Pipes.CustomerSummary;
using Moq;
using NUnit.Framework;

[TestFixture]
public class AddCompanyNumberToRequestTests
    : BaseForPipeTests<CustomerSummaryParameter, CustomerSummaryResult>
{
    private Mock<IntegrationConnectorSettings> integrationConnectorSettings;

    public override Type PipeType => typeof(AddCompanyNumberToRequest);

    public override void SetUp()
    {
        this.integrationConnectorSettings = this.container.GetMock<IntegrationConnectorSettings>();
    }

    [Test]
    public void Order_Is_300()
    {
        Assert.AreEqual(300, this.pipe.Order);
    }

    [TestCase(",")]
    [TestCase(";")]
    [TestCase("|")]
    public void Execute_Should_Split_Company_Number(string delimiter)
    {
        var factsCompanyNumber = $"1{delimiter}2";

        this.WhenFactsCompanyNumberIs(factsCompanyNumber);

        var result = this.RunExecute();

        Assert.AreEqual("1", result.CustomerSummaryRequest.Request.Company);
    }

    [Test]
    public void Execute_Should_Default_Company_Number_When_Prophet21_Company_Number_Is_Empty()
    {
        this.WhenFactsCompanyNumberIs(string.Empty);

        var result = this.RunExecute();

        Assert.AreEqual("01", result.CustomerSummaryRequest.Request.Company);
    }

    protected override CustomerSummaryResult GetDefaultResult()
    {
        return new CustomerSummaryResult
        {
            CustomerSummaryRequest = new CustomerSummaryRequest { Request = new Request() }
        };
    }

    private void WhenFactsCompanyNumberIs(string factsCompanyNumber)
    {
        this.integrationConnectorSettings
            .Setup(o => o.FactsCompanyNumber)
            .Returns(factsCompanyNumber);
    }
}
