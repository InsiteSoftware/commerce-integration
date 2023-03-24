namespace Insite.Integration.Connector.Prophet21.Tests.V2017.Pipelines.GetMyAccountOpenAR;

using System;

using Moq;
using NUnit.Framework;

using Insite.Core.SystemSetting.Groups.Integration;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.Prophet21.V2017.Middleware.Models.GetMyAccountOpenAR;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Parameters;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Pipes.GetMyAccountOpenAR;
using Insite.Integration.Connector.Prophet21.V2017.Pipelines.Results;

public class AddCompanyNumberToRequestTests
    : BaseForPipeTests<GetMyAccountOpenARParameter, GetMyAccountOpenARResult>
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
        var prophet21Company = $"1{delimiter}2";

        this.WhenProphet21CompanyIs(prophet21Company);

        var result = this.RunExecute();

        Assert.AreEqual("1", result.GetMyAccountOpenARRequest.Request.StoreName);
    }

    [Test]
    public void Execute_Should_Default_Company_Number_When_Prophet21_Company_Number_Is_Empty()
    {
        this.WhenProphet21CompanyIs(string.Empty);

        var result = this.RunExecute();

        Assert.AreEqual("01", result.GetMyAccountOpenARRequest.Request.StoreName);
    }

    protected override GetMyAccountOpenARResult GetDefaultResult()
    {
        return new GetMyAccountOpenARResult
        {
            GetMyAccountOpenARRequest = new GetMyAccountOpenAR { Request = new Request() }
        };
    }

    protected void WhenProphet21CompanyIs(string prophet21Company)
    {
        this.integrationConnectorSettings.Setup(o => o.Prophet21Company).Returns(prophet21Company);
    }
}
