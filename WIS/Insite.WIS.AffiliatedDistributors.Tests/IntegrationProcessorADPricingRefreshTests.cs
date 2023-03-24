using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Data;
using Insite.Integration.TestHelpers;
using Insite.WIS.AffiliatedDistributors.Constants;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.Plugins;
using Moq;
using NUnit.Framework;
using DataConstants = Insite.WIS.Broker.Plugins.Constants.Data;
using IntegrationJob = Insite.WIS.Broker.WebIntegrationService.IntegrationJob;
using JobDefinitionStep = Insite.WIS.Broker.WebIntegrationService.JobDefinitionStep;

namespace Insite.WIS.AffiliatedDistributors.Tests;

[TestFixture]
internal class IntegrationProcessorADPricingRefreshTests
{
    private SiteConnection siteConnection;
    private IntegrationJob integrationJob;
    private JobDefinitionStep jobStep;
    private Mock<IFlatFileService> flatFileService;
    private Mock<IntegrationProcessorFlatFile> integrationProcessorFlatFile;

    [SetUp]
    public void Setup()
    {
        var adTestFixture = new IntegrationProcessorADDataFeedTests();
        this.siteConnection = adTestFixture.MockConnection();
        this.integrationJob = adTestFixture.MockJob();
        this.jobStep = adTestFixture.MockJobDefinitionStep();
        this.flatFileService = new Mock<IFlatFileService>();
        this.integrationProcessorFlatFile = new Mock<IntegrationProcessorFlatFile>();

        using (
            var stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream("Insite.WIS.AffiliatedDistributors.Tests.TestData.xml")
        )
        using (var reader = new StreamReader(stream))
        {
            this.integrationJob.InitialData = reader.ReadToEnd();
        }

        var productsTable = this.MockProducts();
        var dataSet = new DataSet();

        this.flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });
        this.flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(productsTable);

        dataSet.Tables.Add(productsTable);

        this.integrationProcessorFlatFile
            .Setup(o => o.Execute(this.siteConnection, this.integrationJob, this.jobStep))
            .Returns(dataSet);
    }

    [Test]
    public void Price_Matrix_Table_Should_Contain_Products()
    {
        var testableProcessor = new TestableIntegrationProcessorADPricingRefresh(
            this.integrationProcessorFlatFile.Object,
            this.flatFileService.Object
        );
        var dataSet = testableProcessor.Execute(
            this.siteConnection,
            this.integrationJob,
            this.jobStep
        );

        var processedMatrix = dataSet.Tables["0PriceMatrix"];

        foreach (DataRow row in processedMatrix.Rows)
        {
            row.Should()
                .HaveColumns(DataConstants.ProductKeyPartColumn, DataConstants.ActivateOnColumn);
        }
    }

    internal DataTable MockProducts(params (string, string)[] optionalColumns)
    {
        var table = new DataTable("4product");

        table.Columns.AddRange(
            new DataColumn[]
            {
                new DataColumn(ADDataFeedSourceFile.MyPartNumberColumn, typeof(string)),
                new DataColumn(ADDataFeedSourceFile.SkuShortDescriptionColumn, typeof(string)),
                new DataColumn(ADDataFeedSourceFile.CAPriceActivationColumn, typeof(string)),
                new DataColumn(ADDataFeedSourceFile.CAPriceDeactivationColumn, typeof(string)),
                new DataColumn(ADDataFeedSourceFile.CAPriceColumn, typeof(string)),
                new DataColumn(ADDataFeedSourceFile.SalesUnitOfMeasureColumn, typeof(string))
            }
        );

        var columnList = new List<DataColumn>();
        foreach (var pair in optionalColumns)
        {
            columnList.Add(new DataColumn(pair.Item1, typeof(string)));
        }

        table.Columns.AddRange(columnList.ToArray());

        var wrenchProduct = table.NewRow();
        var hammerProduct = table.NewRow();

        wrenchProduct[ADDataFeedSourceFile.MyPartNumberColumn] = "BasicProduct_100";
        wrenchProduct[ADDataFeedSourceFile.SkuShortDescriptionColumn] = "BasicProduct_100";
        wrenchProduct[ADDataFeedSourceFile.CAPriceActivationColumn] = DateTime.Now.ToString();
        wrenchProduct[ADDataFeedSourceFile.CAPriceDeactivationColumn] = DateTime.Now
            .AddHours(12)
            .ToString();
        wrenchProduct[ADDataFeedSourceFile.CAPriceColumn] = "100";
        wrenchProduct[ADDataFeedSourceFile.SalesUnitOfMeasureColumn] = "EA";

        hammerProduct[ADDataFeedSourceFile.MyPartNumberColumn] = "111";
        hammerProduct[ADDataFeedSourceFile.SkuShortDescriptionColumn] = "111";
        hammerProduct[ADDataFeedSourceFile.CAPriceActivationColumn] = DateTime.Now;
        hammerProduct[ADDataFeedSourceFile.CAPriceDeactivationColumn] = DateTime.Now.AddHours(12);
        hammerProduct[ADDataFeedSourceFile.CAPriceColumn] = "100";
        hammerProduct[ADDataFeedSourceFile.SalesUnitOfMeasureColumn] = "EA";

        foreach (var pair in optionalColumns)
        {
            wrenchProduct[pair.Item1] = pair.Item2;
            hammerProduct[pair.Item1] = pair.Item2;
        }

        table.Rows.Add(wrenchProduct);
        table.Rows.Add(hammerProduct);

        return table;
    }
}

internal class TestableIntegrationProcessorADPricingRefresh : IntegrationProcessorADPricingRefresh
{
    private readonly IFlatFileService flatFileService;

    public TestableIntegrationProcessorADPricingRefresh(
        IntegrationProcessorFlatFile flatFileIntegrationProcessor,
        IFlatFileService flatFileService
    )
    {
        this.flatFileService = flatFileService;
        this.FlatFileProcessor = flatFileIntegrationProcessor;
    }
}
