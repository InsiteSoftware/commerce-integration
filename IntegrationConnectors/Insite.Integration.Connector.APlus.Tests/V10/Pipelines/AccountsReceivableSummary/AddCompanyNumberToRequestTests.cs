namespace Insite.Integration.Connector.APlus.Tests.V10.Pipelines.AccountsReceivableSummary;

using System;

using Moq;
using NUnit.Framework;

using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.APlus.V10.CommerceGateway.Models.AccountsReceivableSummary;
using Insite.Integration.Connector.APlus.V10.Pipelines.Parameters;
using Insite.Integration.Connector.APlus.V10.Pipelines.Pipes.AccountsReceivableSummary;
using Insite.Integration.Connector.APlus.V10.Pipelines.Results;

[TestFixture]
public class AddCompanyNumberToRequestTests
    : BaseForPipeTests<AccountsReceivableSummaryParameter, AccountsReceivableSummaryResult>
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

    [Test]
    public void Execute_Should_Populate_Company_Number()
    {
        this.WhenAPlusCompanyIs("2,3");

        var result = this.RunExecute();

        Assert.AreEqual("2", result.AccountsReceivableSummaryRequest.CompanyNumber);
    }

    protected override AccountsReceivableSummaryResult GetDefaultResult()
    {
        return new AccountsReceivableSummaryResult
        {
            AccountsReceivableSummaryRequest = new AccountsReceivableSummaryRequest()
        };
    }

    private void WhenAPlusCompanyIs(string aPlusCompany)
    {
        this.integrationConnectorSettings.Setup(o => o.APlusCompany).Returns(aPlusCompany);
    }
}
