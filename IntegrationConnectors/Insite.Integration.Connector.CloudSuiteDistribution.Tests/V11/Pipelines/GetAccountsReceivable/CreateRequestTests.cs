namespace Insite.Integration.Connector.CloudSuiteDistribution.Tests.V11.Pipelines.GetAccountsReceivable;

using System;
using FluentAssertions;
using Insite.Common.Helpers;
using Insite.Core.Plugins.Customer;
using Insite.Core.TestHelpers.Builders;
using Insite.Core.TestHelpers.Pipelines;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Parameters;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Pipes.GetAccountsReceivable;
using Insite.Integration.Connector.CloudSuiteDistribution.V11.Pipelines.Results;
using NUnit.Framework;

[TestFixture]
public class CreateRequestTests
    : BaseForPipeTests<GetAccountsReceivableParameter, GetAccountsReceivableResult>
{
    public override Type PipeType => typeof(CreateRequest);

    public override void SetUp() { }

    [Test]
    public void Order_Is_100()
    {
        this.pipe.Order.Should().Be(100);
    }

    [Test]
    public void Execute_Should_Set_Credentials_From_IntegrationConnection()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var integrationConnection = Some.IntegrationConnection()
            .WithSystemNumber("1")
            .WithLogOn("web")
            .WithPassword(EncryptionHelper.EncryptAes("password"))
            .Build();
#pragma warning restore

        var parameter = new GetAccountsReceivableParameter
        {
            IntegrationConnection = integrationConnection
        };

        var result = this.RunExecute(parameter);

        result.SfCustomerSummaryRequest.Request.CompanyNumber.Should().Be(1);
        result.SfCustomerSummaryRequest.Request.OperatorInit
            .Should()
            .Be(integrationConnection.LogOn);
#pragma warning disable CS0618 // Type or member is obsolete
        result.SfCustomerSummaryRequest.Request.OperatorPassword
            .Should()
            .Be(EncryptionHelper.DecryptAes(integrationConnection.Password));
#pragma warning restore
    }

    [Test]
    public void Execute_Should_Set_CustomerNumber_From_Parameter_Customer_ErpNumber()
    {
        var parameter = new GetAccountsReceivableParameter
        {
            GetAgingBucketsParameter = new GetAgingBucketsParameter
            {
                Customer = Some.Customer().WithErpNumber("ABC123").Build()
            }
        };

        var result = this.RunExecute(parameter);

        result.SfCustomerSummaryRequest.Request.CustomerNumber
            .Should()
            .Be(parameter.GetAgingBucketsParameter.Customer.ErpNumber);
    }
}
