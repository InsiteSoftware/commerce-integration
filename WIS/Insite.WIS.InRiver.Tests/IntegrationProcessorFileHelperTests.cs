namespace Insite.WIS.InRiver.Tests;

using System.IO;
using Insite.WIS.Broker.WebIntegrationService;
using NUnit.Framework;

[TestFixture]
public class IntegrationProcessorFileHelperTests
{
    [SetUp]
    public void Setup()
    {
        Directory.CreateDirectory(@"C:\Temp");
        Directory.CreateDirectory(@"C:\Temp\inRiver");

        this.integrationProcessorFileHelper = new IntegrationProcessorFileHelper();
    }

    [TearDown]
    public void Teardown()
    {
        Directory.Delete(@"C:\Temp\inRiver", true);
    }

    private IntegrationProcessorFileHelper integrationProcessorFileHelper;

    private const string XmlFilePath = @"C:\Temp\inRiver\test.xml";

    private const string ProcessingFilePath = @"C:\Temp\inRiver\test.xml.1.processing";

    private const string ProcessedFilePath = @"C:\Temp\inRiver\test.xml.1.processed";

    [TestCase(true)]
    [TestCase(false)]
    public void MoveFileToProcessed_Should_Move_File_To_Processed_If_Not_Already_Processed(
        bool isPreview
    )
    {
        CreateFile(XmlFilePath);
        var integrationJob = new IntegrationJob
        {
            JobNumber = 1,
            IsPreview = isPreview,
            JobDefinition = new JobDefinition()
            {
                IntegrationConnection = new IntegrationConnection()
            }
        };

        var processingFileName = this.MoveFileForProcessing(XmlFilePath, integrationJob);
        this.integrationProcessorFileHelper.MoveFileToProcessed(
            integrationJob,
            processingFileName,
            XmlFilePath
        );

        Assert.IsTrue(isPreview ? File.Exists(XmlFilePath) : File.Exists(ProcessedFilePath));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void MoveFileToProcessed_Should_Not_Move_File_To_Processed_If_Already_Processed(
        bool isPreview
    )
    {
        CreateFile(ProcessedFilePath);
        var integrationJob = new IntegrationJob
        {
            JobNumber = 1,
            IsPreview = isPreview,
            JobDefinition = new JobDefinition()
            {
                IntegrationConnection = new IntegrationConnection()
            }
        };

        var processingFileName = this.MoveFileForProcessing(ProcessedFilePath, integrationJob);
        this.integrationProcessorFileHelper.MoveFileToProcessed(
            integrationJob,
            processingFileName,
            ProcessedFilePath
        );

        Assert.IsTrue(File.Exists(ProcessedFilePath));
    }

    private static void CreateFile(string filePath)
    {
        var file = new FileInfo(filePath);
        using (var stream = file.Create()) { }
    }

    private string MoveFileForProcessing(string filePath, IntegrationJob integrationJob)
    {
        return this.integrationProcessorFileHelper.MoveFileForProcessing(filePath, integrationJob);
    }

    [Test]
    public void MoveFileForProcessing_Should_Rename_File_If_Not_Exists()
    {
        CreateFile(XmlFilePath);
        const int jobNumber = 1;
        var integrationJob = new IntegrationJob
        {
            JobNumber = jobNumber,
            JobDefinition = new JobDefinition()
            {
                IntegrationConnection = new IntegrationConnection()
            }
        };

        var processingFileName = this.MoveFileForProcessing(XmlFilePath, integrationJob);

        Assert.AreEqual(ProcessingFilePath, processingFileName);
    }

    [Test]
    public void MoveFileForProcessing_Should_Return_File_If_Exists()
    {
        CreateFile(ProcessedFilePath);
        var integrationJob = new IntegrationJob
        {
            JobNumber = 1,
            JobDefinition = new JobDefinition()
            {
                IntegrationConnection = new IntegrationConnection()
            }
        };

        var processingFileName = this.MoveFileForProcessing(ProcessedFilePath, integrationJob);

        Assert.AreEqual(ProcessedFilePath, processingFileName);
    }

    [Test]
    public void ProcessBadFile_Should_Move_File_To_Directory()
    {
        CreateFile(XmlFilePath);

        this.integrationProcessorFileHelper.ProcessBadFile(
            XmlFilePath,
            new IntegrationConnection()
        );

        Assert.IsTrue(File.Exists(@"C:\Temp\inRiver\BadFiles\test.xml"));
    }

    [Test]
    public void RetrieveFilesForProcessing_Should_Return_List_Of_Files()
    {
        CreateFile(XmlFilePath);
        var integrationJob = new IntegrationJob
        {
            JobNumber = 1,
            JobDefinition = new JobDefinition()
            {
                IntegrationConnection = new IntegrationConnection()
            }
        };
        var jobStep = new JobDefinitionStep { FromClause = "*.xml" };
        var integrationConnection = new IntegrationConnection { Url = @"C:\Temp\inRiver" };

        var files = this.integrationProcessorFileHelper.RetrieveFilesForProcessing(
            integrationJob,
            jobStep,
            integrationConnection
        );

        Assert.IsTrue(files.Contains(XmlFilePath));
    }
}
