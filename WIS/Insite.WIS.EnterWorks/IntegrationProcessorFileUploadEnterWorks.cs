namespace Insite.WIS.EnterWorks;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using Insite.WIS.Broker.Parsers;
using Insite.WIS.Broker.Plugins;

public class IntegrationProcessorFileUploadEnterWorks : IntegrationProcessorFileUpload
{
    private const string FilePathColumn = "FilePath";

    private const string ParentDirectoriesParameter = "ParentDirectories";

    private const string ZipFileExtension = ".zip";

    protected override IEnumerable<string> GetSourceFilePaths(
        FileUploadParameter fileUploadParameter
    )
    {
        var enterWorksFileImportSourceFilePaths = base.GetSourceFilePaths(fileUploadParameter);
        var enterWorksFileImportDataTables = this.ParseEnterWorksFileImports(
            enterWorksFileImportSourceFilePaths
        );

        return this.GetEnterWorksSourceFilePaths(enterWorksFileImportDataTables);
    }

    protected override string GetDestinationFilePath(
        FileUploadParameter fileUploadParameter,
        string sourceFilePath
    )
    {
        return this.FileProvider.CombinePaths(
            fileUploadParameter.DestinationDirectory,
            sourceFilePath
        );
    }

    private IEnumerable<DataTable> ParseEnterWorksFileImports(
        IEnumerable<string> enterWorksFileImportSourceFilePaths
    )
    {
        var fileParser = new FileParserGemBox(this.IntegrationJob, this.JobDefinitionStep);
        var dataTables = new List<DataTable>();

        foreach (var enterWorksFileImportSourceFilePath in enterWorksFileImportSourceFilePaths)
        {
            using (var stream = this.FileProvider.GetFile(enterWorksFileImportSourceFilePath))
            {
                if (enterWorksFileImportSourceFilePath.EndsWithIgnoreCase(ZipFileExtension))
                {
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            dataTables.Add(fileParser.ParseFile(entry.Open(), entry.FullName));
                        }
                    }
                }
                else
                {
                    dataTables.Add(
                        fileParser.ParseFile(stream, enterWorksFileImportSourceFilePath)
                    );
                }
            }
        }

        return dataTables;
    }

    private IEnumerable<string> GetEnterWorksSourceFilePaths(
        IEnumerable<DataTable> enterWorksFileImportDataTables
    )
    {
        var parentDirectories = this.ParseParentDirectoriesParameter();
        var sourceFilePaths = new List<string>();

        foreach (var enterWorksFileImportDataTable in enterWorksFileImportDataTables)
        {
            foreach (
                var dataRow in enterWorksFileImportDataTable.Select(
                    this.JobDefinitionStep.WhereClause
                )
            )
            {
                var filePath = dataRow[FilePathColumn].ToString();

                foreach (var parentDirectory in parentDirectories)
                {
                    var sourceFilePath = this.FileProvider.CombinePaths(parentDirectory, filePath);
                    if (this.FileProvider.FileExists(sourceFilePath))
                    {
                        sourceFilePaths.Add(sourceFilePath);
                    }
                }
            }
        }

        return sourceFilePaths;
    }

    private IEnumerable<string> ParseParentDirectoriesParameter()
    {
        var parentDirectoriesParameter =
            this.IntegrationJob.IntegrationJobParameters.FirstOrDefault(
                p => p.JobDefinitionParameter.Name.EqualsIgnoreCase(ParentDirectoriesParameter)
            );

        return parentDirectoriesParameter?.Value.Split(
                new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries
            ) ?? new string[0];
    }
}
