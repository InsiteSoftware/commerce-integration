namespace Insite.WIS.InRiver.Tests.Services.FileServiceTypes.Azure;

using FluentAssertions;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.InRiver.Services.FileServiceTypes.Azure;
using Insite.WIS.InRiver.Services.FileServiceTypes.Azure.Factories;
using Insite.WIS.InRiver.Services.FileServiceTypes.Azure.Providers;
using Moq;
using NUnit.Framework;
using System;

[TestFixture]
public class AzureFileServiceTests
{
    private Mock<IAzureProvider> mockIAzureProvider;
    private AzureFileService azureFileService;

    [SetUp]
    public void SetUp()
    {
        var factory = new Mock<IAzureProviderFactory>();
        var provider = this.mockIAzureProvider = new Mock<IAzureProvider>();
        factory
            .Setup(o => o.CreateAzureProvider(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(provider.Object);
        this.azureFileService = new AzureFileService(factory.Object);
    }

    private const string BaseUrl =
        "https://blobteststorageinsite.blob.core.windows.net/dataexchange/";

    private static IntegrationJob CreateIntegrationJob() =>
        new IntegrationJob
        {
            JobDefinition = new JobDefinition
            {
                IntegrationConnection = new IntegrationConnection
                {
                    ArchiveFolder = "Publish_Archive/Common/SchemaBasedChannelListener/CVLs/",
                    Url = "Publish/Common/SchemaBasedChannelListener/CVLs/",
                },
            },
        };

    [Test]
    public void AzureFileService_MoveToProcessed_Sends_To_Archive_Folder()
    {
        var job = CreateIntegrationJob();
        var inputFolder = job.JobDefinition.IntegrationConnection.Url;
        var fileName = BaseUrl + inputFolder + "Input.xml";
        var processingFileName = fileName + ".0.processing";
        var archiveFileName =
            fileName.Replace(inputFolder, job.JobDefinition.IntegrationConnection.ArchiveFolder)
            + ".0.processed";

        this.mockIAzureProvider
            .Setup(o => o.MoveBlob(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>(
                (source, destination) => destination.Should().Be(archiveFileName)
            );

        this.azureFileService.MoveFileToProcessed(job, processingFileName, fileName);

        this.mockIAzureProvider.Verify(
            o => o.MoveBlob(It.IsAny<string>(), It.IsAny<string>()),
            Times.Once()
        );
    }

    [Test]
    public void AzureFileService_MoveToProcessed_Tolerates_Missing_Archive_Folder()
    {
        var job = CreateIntegrationJob();
        job.JobDefinition.IntegrationConnection.ArchiveFolder = null;
        var inputFolder = job.JobDefinition.IntegrationConnection.Url;
        var fileName = BaseUrl + inputFolder + "Input.xml";
        var processingFileName = fileName + ".0.processing";
        var archiveFileName = fileName + ".0.processed";

        this.mockIAzureProvider
            .Setup(o => o.MoveBlob(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>(
                (source, destination) => destination.Should().Be(archiveFileName)
            );

        this.azureFileService.MoveFileToProcessed(job, processingFileName, fileName);

        this.mockIAzureProvider.Verify(
            o => o.MoveBlob(It.IsAny<string>(), It.IsAny<string>()),
            Times.Once()
        );
    }

    [Test]
    public void AzureFileService_MoveToProcessed_Throws_Exception_If_Folder_Mismatch()
    {
        var job = CreateIntegrationJob();
        var inputFolder = job.JobDefinition.IntegrationConnection.Url;
        var fileName = BaseUrl + inputFolder + "Input.xml";
        var processingFileName = fileName + ".0.processing";
        var archiveFileName =
            fileName.Replace(inputFolder, job.JobDefinition.IntegrationConnection.ArchiveFolder)
            + ".0.processed";

        this.mockIAzureProvider
            .Setup(o => o.MoveBlob(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>(
                (source, destination) => destination.Should().Be(archiveFileName)
            );

        this.azureFileService
            .Invoking(
                az => az.MoveFileToProcessed(job, processingFileName, fileName.ToLowerInvariant())
            )
            .Should()
            .Throw<InvalidOperationException>()
            .WithMessage(
                "*match*",
                "Our Azure provider was written based on a list of file names provided by Azure, which may resolve case differences on the user's behalf."
            );

        this.mockIAzureProvider.Verify(
            o => o.MoveBlob(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never()
        );
    }
}
