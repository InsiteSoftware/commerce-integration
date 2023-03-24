namespace Insite.WIS.AffiliatedDistributors.Tests;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FluentAssertions;
using Insite.Integration.Enums;
using Insite.Integration.TestHelpers;
using Insite.WIS.AffiliatedDistributors.Constants;
using Insite.WIS.Broker;
using Insite.WIS.Broker.Interfaces;
using Insite.WIS.Broker.WebIntegrationService;
using Microsoft.OData;
using Moq;
using NUnit.Framework;

[TestFixture]
public class IntegrationProcessorADDataFeedTests
{
    [TestCase("ATTRIBUTE_VALUE_")]
    [TestCase("ATTRIBUTE VALUE ")]
    [TestCase("Value_UOM_")]
    [TestCase("Value UOM ")]
    public void Execute_Should_Continue_Processing_DataSet_When_Attribute_Value_Is_Blank(
        string valueColumnName
    )
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            ($"{ADDataFeedSourceFile.AttributeNameColumn}_1", "Cordless/ Corded"),
            ($"{valueColumnName}1", "Corded"),
            ($"{ADDataFeedSourceFile.AttributeNameColumn}_2", "Voltage"),
            ($"{valueColumnName}2", string.Empty),
            ($"{ADDataFeedSourceFile.AttributeNameColumn}_3", "Blade Diameter"),
            ($"{valueColumnName}3", "7-1/4")
        );
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            this.MockJob(),
            this.MockJobDefinitionStep()
        );

        var attributeValueTable = dataSet.Tables["7AttributeValue"];
        attributeValueTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[] { "Cordless/ Corded", "Corded", 1 },
                    new object[] { "Blade Diameter", "7-1/4", 2 },
                }
            );

        var productAttributeValueTable = dataSet.Tables["8ProductAttributeValue"];
        productAttributeValueTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[] { "SAW123", "Cordless/ Corded", "Corded" },
                    new object[] { "SAW123", "Blade Diameter", "7-1/4" },
                }
            );
    }

    [Test]
    public void Execute_Should_Create_Image_Tables_For_Enterworks()
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            ($"{ADDataFeedSourceFile.ItemImageFilenameColumn}_1", "img.jpg")
        );

        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            GetIntegrationJob(imagesParameterValue: "Enterworks"),
            this.MockJobDefinitionStep(
                parameters: new[]
                {
                    new JobDefinitionStepParameter { Name = "Images", Value = "Enterworks" }
                }
            )
        );

        var productImageTable = dataSet.Tables["10ProductImage"];
        productImageTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[]
                    {
                        "SAW123",
                        "/userfiles/ad/small/img.jpg",
                        "/userfiles/ad/medium/img.jpg",
                        "/userfiles/ad/large/img.jpg",
                        1
                    }
                }
            );
    }

    [Test]
    public void Execute_Should_Create_Category_Image_Table()
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = new DataTable();
        table.Columns.Add(ADDataFeedSourceFile.CategoryImageName, typeof(string));
        table.Columns.Add(ADDataFeedSourceFile.NumericCodeColumn, typeof(string));
        table.Columns.Add(ADDataFeedSourceFile.LevelNumber, typeof(string));
        table.Columns.Add("Level1Code", typeof(string));
        table.Rows.Add(
            "https://protect-de.mimecast.com/s/cbyMC28yJVsR4r65hk2EUb?",
            "1000000000",
            "1",
            "1000000000"
        );
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var job = GetIntegrationJob(websiteParameterValue: "TomsTrunk");

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            job,
            new JobDefinitionStep
            {
                ObjectName = "Category",
                JobDefinitionStepParameters = new JobDefinitionStepParameter[] { },
                Name = "refresh category images"
            }
        );

        var categoryImageTable = dataSet.Tables["16Category"];
        categoryImageTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[]
                    {
                        "TomsTrunk:1000000000",
                        "https://protect-de.mimecast.com/s/cbyMC28yJVsR4r65hk2EUb?"
                    }
                }
            );
    }

    [Test]
    public void Execute_Should_Map_Paths_For_Enterworks_Files_When_Mapping_Parameters_Have_Values()
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            ($"{ADDataFeedSourceFile.ItemImageFilenameColumn}_1", "img.jpg")
        );
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            GetIntegrationJob(
                imagesParameterValue: "Enterworks",
                sourceFolderParameterValue: "images/adtest",
                smallFolderParameterValue: "path/to/small",
                mediumFolderParameterValue: "path/to/medium",
                largeFolderParameterValue: "path/to/large"
            ),
            this.MockJobDefinitionStep()
        );

        var productImageTable = dataSet.Tables["10ProductImage"];
        productImageTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[]
                    {
                        "SAW123",
                        "/userfiles/images/adtest/path/to/small/img.jpg",
                        "/userfiles/images/adtest/path/to/medium/img.jpg",
                        "/userfiles/images/adtest/path/to/large/img.jpg",
                        1
                    }
                }
            );
    }

    [Test]
    public void Execute_Should_Map_Paths_For_Enterworks_Files_When_Mapping_Parameters_Have_Values_Test_Small_And_Medium_Images_to_Jpg()
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            ($"{ADDataFeedSourceFile.ItemImageFilenameColumn}_1", "img.jpg")
        );
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            GetIntegrationJob(
                imagesParameterValue: "Enterworks",
                sourceFolderParameterValue: "images/adtest",
                smallFolderParameterValue: "path/to/small",
                mediumFolderParameterValue: "path/to/medium",
                largeFolderParameterValue: "path/to/large"
            ),
            this.MockJobDefinitionStep()
        );

        var productImageTable = dataSet.Tables["10ProductImage"];
        foreach (DataRow row in productImageTable.Rows)
        {
            row.Should()
                .HaveColumns(
                    ADDataFeedSourceFile.MyPartNumberColumn,
                    "Image_Small",
                    "Image_Medium",
                    "Image_Large",
                    "SortOrder"
                );
        }
    }

    [Test]
    public void Execute_Should_Not_Map_Paths_For_Enterworks_Files_That_Use_Full_Url_Test_Small_And_Medium_Images_to_Jpg()
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            ($"{ADDataFeedSourceFile.ItemImageFilenameColumn}_1", "http://localhost/img.png")
        );
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            GetIntegrationJob(
                imagesParameterValue: "Enterworks",
                sourceFolderParameterValue: "images/adtest",
                smallFolderParameterValue: "path/to/small",
                mediumFolderParameterValue: "path/to/medium",
                largeFolderParameterValue: "path/to/large"
            ),
            this.MockJobDefinitionStep()
        );

        var productImageTable = dataSet.Tables["10ProductImage"];
        productImageTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[]
                    {
                        "SAW123",
                        "http://localhost/img.jpg",
                        "http://localhost/img.jpg",
                        "http://localhost/img.png",
                        1
                    }
                }
            );
    }

    [Test]
    public void Execute_Should_Not_Map_Paths_For_Enterworks_Files_That_Use_Full_Url()
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            ($"{ADDataFeedSourceFile.ItemImageFilenameColumn}_1", "http://localhost/img.jpg")
        );
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            GetIntegrationJob(
                imagesParameterValue: "Enterworks",
                sourceFolderParameterValue: "images/adtest",
                smallFolderParameterValue: "path/to/small",
                mediumFolderParameterValue: "path/to/medium",
                largeFolderParameterValue: "path/to/large"
            ),
            this.MockJobDefinitionStep()
        );

        var productImageTable = dataSet.Tables["10ProductImage"];
        productImageTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[]
                    {
                        "SAW123",
                        "http://localhost/img.jpg",
                        "http://localhost/img.jpg",
                        "http://localhost/img.jpg",
                        1
                    }
                }
            );
    }

    [Test]
    public void Execute_Should_Create_Image_Tables_For_Non_Enterworks()
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            ($"{ADDataFeedSourceFile.ItemImageItemImageColumn}1", "img.jpg"),
            (
                $"{ADDataFeedSourceFile.ItemImageDetailImageColumn}1",
                "http://trunk.local.com/img2.jpg"
            ),
            (
                $"{ADDataFeedSourceFile.ItemImageEnlargeImageColumn}1",
                "https://trunk.local.com/img3.jpg"
            ),
            ($"{ADDataFeedSourceFile.ItemImageCaptionColumn}_1", "img4.jpg")
        );

        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            new IntegrationJob
            {
                IntegrationJobParameters = new IntegrationJobParameter[] { },
                JobDefinition = new JobDefinition
                {
                    IntegrationConnection = new IntegrationConnection()
                }
            },
            this.MockJobDefinitionStep(
                parameters: new[]
                {
                    new JobDefinitionStepParameter { Name = "Images", Value = "NoEnterworks" }
                }
            )
        );

        var productImageTable = dataSet.Tables["10ProductImage"];
        productImageTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[]
                    {
                        "SAW123",
                        "/userfiles/images/products/i/img.jpg",
                        "http://trunk.local.com/img2.jpg",
                        "https://trunk.local.com/img3.jpg",
                        "img4.jpg",
                        1
                    }
                }
            );
    }

    [TestCase("360_IMAGE")]
    [TestCase("360_Spin_URL")]
    public void Execute_Should_Handle_Different_360_Image_Columns(string valueColumnName)
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            (valueColumnName, "img.jpg")
        );
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            this.MockJob(),
            this.MockJobDefinitionStep()
        );

        var attributeValueTable = dataSet.Tables["14ProductImage"];
        attributeValueTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]> { new object[] { "SAW123", "img.jpg" } }
            );
    }

    [TestCase("manual.pdf", "/userfiles/documents/manual.pdf")]
    [TestCase("http://localhost/manual.pdf", "http://localhost/manual.pdf")]
    public void Execute_Should_Create_Document_Tables_With_Non_Mapped_Paths(
        string filename,
        string expectedPath
    )
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            ($"{ADDataFeedSourceFile.ItemDocumentNameColumn}_1", filename),
            ($"{ADDataFeedSourceFile.ItemDocumentTypeColumn}_1", null),
            ($"{ADDataFeedSourceFile.DocCaptionColumn}_1", null)
        );
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            GetIntegrationJob(),
            this.MockJobDefinitionStep()
        );

        var documentTable = dataSet.Tables["11Document"];
        documentTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[] { "SAW123", "Product", expectedPath, string.Empty, string.Empty }
                }
            );
    }

    [TestCase("manual.pdf", "/userfiles/documents/manual.pdf")]
    [TestCase("http://localhost/manual.pdf", "http://localhost/manual.pdf")]
    public void Execute_Should_Create_Document_Tables_With_Non_Mapped_Paths_For_Enterworks_Data_File_Format(
        string filename,
        string expectedPath
    )
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            ($"{ADDataFeedSourceFile.EnterworksItemDocumentNameColumn}_1", filename),
            ($"{ADDataFeedSourceFile.EnterworksDocCaptionColumn}_1", "Power saw manual")
        );
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            GetIntegrationJob(),
            this.MockJobDefinitionStep()
        );

        var documentTable = dataSet.Tables["11Document"];
        documentTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[]
                    {
                        "SAW123",
                        "Product",
                        expectedPath,
                        string.Empty,
                        "Power saw manual"
                    }
                }
            );
    }

    [Test]
    public void Execute_Should_Map_Paths_For_Documents_When_Mapping_Parameters_Have_Values()
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            ($"{ADDataFeedSourceFile.ItemDocumentNameColumn}_1", "img.jpg"),
            ($"{ADDataFeedSourceFile.ItemDocumentTypeColumn}_1", null),
            ($"{ADDataFeedSourceFile.DocCaptionColumn}_1", null)
        );
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            GetIntegrationJob(
                sourceFolderParameterValue: "files/adtest",
                documentFolderParameterValue: "path/to/documents"
            ),
            this.MockJobDefinitionStep()
        );

        var documentTable = dataSet.Tables["11Document"];
        foreach (DataRow row in documentTable.Rows)
        {
            row.Should()
                .HaveColumns(
                    ADDataFeedSourceFile.MyPartNumberColumn,
                    "ParentTable",
                    "ITEM_DOCUMENT_NAME",
                    "ITEM_DOCUMENT_TYPE",
                    "DOC_CAPTION"
                );
        }
    }

    [Test]
    public void Execute_Should_Map_Paths_For_Documents_When_Mapping_Parameters_Have_Values_For_Enterworks_Data_File_Format()
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            ($"{ADDataFeedSourceFile.EnterworksItemDocumentNameColumn}_1", "manual.pdf"),
            ($"{ADDataFeedSourceFile.EnterworksDocCaptionColumn}_1", "Power saw manual")
        );
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            GetIntegrationJob(
                sourceFolderParameterValue: "files/adtest",
                documentFolderParameterValue: "path/to/documents"
            ),
            this.MockJobDefinitionStep()
        );

        var documentTable = dataSet.Tables["11Document"];
        documentTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[]
                    {
                        "SAW123",
                        "Product",
                        "/userfiles/files/adtest/path/to/documents/manual.pdf",
                        string.Empty,
                        "Power saw manual"
                    }
                }
            );
    }

    [Test]
    public void Execute_StyleTrait_Data_Table_Process()
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });
        var table = new DataTable();
        table.Columns.Add(ADDataFeedSourceFile.NumericCodeColumn, typeof(string));
        table.Columns.Add($"{ADDataFeedSourceFile.NodeNameColumn}", typeof(string));
        table.Columns.Add($"{ADDataFeedSourceFile.CategoryAttributeNameColumn}", typeof(string));
        table.Columns.Add($"{ADDataFeedSourceFile.DisplaySequenceColumn}", typeof(string));
        table.Rows.Add("1010100000", "Coated Abrasive Belts", "Abrasive Material", "1");
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            GetIntegrationJob(
                sourceFolderParameterValue: "files/adtest",
                documentFolderParameterValue: "path/to/documents"
            ),
            this.MockJobDefinitionStep("styleclass")
        );

        var documentTable = dataSet.Tables["2StyleTrait"];
        documentTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]> { new object[] { "1010100000", "Abrasive Material", "1" } }
            );
    }

    [Test]
    public void Execute_Create_A_New_Row_If_Product_If_It_Has_A_Product_Name_Checking_StyleTraitValueTable()
    {
        var styleFlatFileService = new Mock<IFlatFileService>();
        styleFlatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var styleTable = new DataTable();
        styleTable.Columns.Add(ADDataFeedSourceFile.NumericCodeColumn, typeof(string));
        styleTable.Columns.Add($"{ADDataFeedSourceFile.NodeNameColumn}", typeof(string));
        styleTable.Columns.Add(
            $"{ADDataFeedSourceFile.CategoryAttributeNameColumn}",
            typeof(string)
        );
        styleTable.Columns.Add($"{ADDataFeedSourceFile.DisplaySequenceColumn}", typeof(string));
        styleTable.Rows.Add("3713310000", "Coated Abrasive Belts", "Abrasive Material", "1");
        styleFlatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(styleTable);
        styleFlatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });
        var integrationProcessADDataFeed = new TestableIntegrationProcessorADDataFeed(
            styleFlatFileService.Object
        );

        var siteConnection = this.MockConnection();
        var styledataSet = integrationProcessADDataFeed.Execute(
            siteConnection,
            GetIntegrationJob(
                sourceFolderParameterValue: "files/adtest",
                documentFolderParameterValue: "path/to/documents"
            ),
            this.MockJobDefinitionStep("styleclass")
        );

        var table = this.MockProducts(
            ("Member SKU ID", "61780808"),
            ("AD SKU ID", "Lochivar - Q1 - 2018 - PLB - 1544"),
            (ADDataFeedSourceFile.MyPartNumberColumn, "TOM AD1"),
            ("Product_Name", "Datta Product"),
            ("Product_Group", "Datta Group"),
            ("Category_Code", "3713310000"),
            ("Attribute Name 1", "Abrasive Material"),
            ("Value UOM 1", "123")
        );
        styleFlatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = integrationProcessADDataFeed.Execute(
            siteConnection,
            GetIntegrationJob(
                sourceFolderParameterValue: "files/adtest",
                documentFolderParameterValue: "path/to/documents"
            ),
            this.MockJobDefinitionStep()
        );

        var styleTrailTable = dataSet.Tables["3StyleTraitValue"];
        foreach (DataRow row in styleTrailTable.Rows)
        {
            row.Should().HaveColumns("StyleClass", "StyleTrait", "StyleTraitValue");
        }
    }

    [TestCase(
        "Member_Part_Number",
        "CATEGORY_CODE_1_LEVEL_1",
        "CATEGORY_NAME_1_LEVEL_1",
        "CATEGORY_CODE_1_LEVEL_2",
        "CATEGORY_NAME_1_LEVEL_2"
    )]
    // This AD data file format starts with category column names that don't have a number suffix.
    [TestCase(
        "Member Part Number",
        "CATEGORY_CODE_LEVEL_1",
        "CATEGORY_NAME_LEVEL_1",
        "CATEGORY_CODE_LEVEL_2",
        "CATEGORY_NAME_LEVEL_2"
    )]
    public void Execute_Should_If_Product_Has_A_LanguageCode(
        string partNumberColumn,
        string categoryCodeLevel1Column,
        string categoryNameLevel1Column,
        string categoryCodeLevel2Column,
        string categoryNameLevel2Column
    )
    {
        var processor = ConfigureProcessor(
            new (string, DataTable)[]
            {
                (
                    "AD.csv",
                    this.MockProducts(
                        (partNumberColumn, "WRENCH-1234"),
                        (categoryCodeLevel1Column, "HANDTOOLS"),
                        (categoryNameLevel1Column, "Hand Tools"),
                        (categoryCodeLevel2Column, "WRENCHES"),
                        (categoryNameLevel2Column, "Wrenches")
                    )
                )
            }
        );

        var dataSet = processor.Execute(
            this.MockConnection(),
            this.MockJob(
                new IntegrationJobParameter[]
                {
                    new IntegrationJobParameter
                    {
                        JobDefinitionParameter = new JobDefinitionParameter
                        {
                            Name = "LanguageCode"
                        },
                        Value = "FIR"
                    }
                }
            ),
            this.MockJobDefinitionStep()
        );

        var processedCategories = dataSet.Tables["4product"];
        foreach (DataRow row in processedCategories.Rows)
        {
            row.Should()
                .HaveColumns(
                    ADDataFeedSourceFile.SkuShortDescriptionColumn,
                    ADDataFeedSourceFile.MyPartNumberColumn
                );
        }
    }

    [TestCase(
        "Member_Part_Number",
        "CATEGORY_CODE_1_LEVEL_1",
        "CATEGORY_NAME_1_LEVEL_1",
        "CATEGORY_CODE_1_LEVEL_2",
        "CATEGORY_NAME_1_LEVEL_2"
    )]
    // This AD data file format starts with category column names that don't have a number suffix.
    [TestCase(
        "Member Part Number",
        "CATEGORY_CODE_LEVEL_1",
        "CATEGORY_NAME_LEVEL_1",
        "CATEGORY_CODE_LEVEL_2",
        "CATEGORY_NAME_LEVEL_2"
    )]
    public void Execute_Should_Combine_SKULongDescription1Column_And_SKULongDescription2Column_If_Mapping_From_SKULongDescription1Column_To_MetaData_Translation(
        string partNumberColumn,
        string categoryCodeLevel1Column,
        string categoryNameLevel1Column,
        string categoryCodeLevel2Column,
        string categoryNameLevel2Column
    )
    {
        var processor = ConfigureProcessor(
            new (string, DataTable)[]
            {
                (
                    "AD.csv",
                    this.MockProducts(
                        (partNumberColumn, "WRENCH-1234"),
                        (categoryCodeLevel1Column, "HANDTOOLS"),
                        (categoryNameLevel1Column, "Hand Tools"),
                        (categoryCodeLevel2Column, "WRENCHES"),
                        (categoryNameLevel2Column, "Wrenches")
                    )
                )
            }
        );

        var dataSet = processor.Execute(
            this.MockConnection(),
            this.MockJob(
                new IntegrationJobParameter[]
                {
                    new IntegrationJobParameter
                    {
                        JobDefinitionParameter = new JobDefinitionParameter
                        {
                            Name = "LanguageCode"
                        },
                        Value = "FR"
                    }
                }
            ),
            this.MockJobDefinitionStep()
        );

        var dataTable = dataSet.Tables["4product"];
        dataTable.Should().NotBeNull();
        dataTable.Columns[ADDataFeedSourceFile.SKULongDescription1Column].Equals(
            "Example Long 1, Example Long 2"
        );
    }

    [Test]
    public void Execute_Create_A_New_Row_If_Product_If_It_Has_A_Product_Name_Checking_StyleTraitValueProductTable()
    {
        var styleFlatFileService = new Mock<IFlatFileService>();
        styleFlatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });
        var styleTable = new DataTable();
        styleTable.Columns.Add(ADDataFeedSourceFile.NumericCodeColumn, typeof(string));
        styleTable.Columns.Add($"{ADDataFeedSourceFile.NodeNameColumn}", typeof(string));
        styleTable.Columns.Add(
            $"{ADDataFeedSourceFile.CategoryAttributeNameColumn}",
            typeof(string)
        );
        styleTable.Columns.Add($"{ADDataFeedSourceFile.DisplaySequenceColumn}", typeof(string));
        styleTable.Rows.Add("3713310000", "Coated Abrasive Belts", "Abrasive Material", "1");
        styleFlatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(styleTable);
        var integrationProcessADDataFeed = new TestableIntegrationProcessorADDataFeed(
            styleFlatFileService.Object
        );

        var siteConnection = this.MockConnection();
        var styledataSet = integrationProcessADDataFeed.Execute(
            siteConnection,
            GetIntegrationJob(
                sourceFolderParameterValue: "files/adtest",
                documentFolderParameterValue: "path/to/documents"
            ),
            this.MockJobDefinitionStep("styleclass")
        );

        styleFlatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            ("Member SKU ID", "61780808"),
            ("AD SKU ID", "Lochivar - Q1 - 2018 - PLB - 1544"),
            (ADDataFeedSourceFile.MyPartNumberColumn, "TOM AD1"),
            ("Product_Name", "Datta Product"),
            ("Product_Group", "Datta Group"),
            ("Category_Code", "3713310000"),
            ("Attribute Name 1", "Abrasive Material"),
            ("Value UOM 1", "123")
        );

        styleFlatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = integrationProcessADDataFeed.Execute(
            siteConnection,
            GetIntegrationJob(
                sourceFolderParameterValue: "files/adtest",
                documentFolderParameterValue: "path/to/documents"
            ),
            this.MockJobDefinitionStep()
        );

        var documentTable = dataSet.Tables["5StyleTraitValueProduct"];
        foreach (DataRow row in documentTable.Rows)
        {
            row.Should()
                .HaveColumns(
                    ADDataFeedSourceFile.MyPartNumberColumn,
                    "StyleClass",
                    "StyleTrait",
                    "StyleTraitValue"
                );
        }
    }

    [Test]
    public void Execute_Should_Not_Map_Paths_For_Documents_That_Use_Full_Url()
    {
        var styleFlatFileService = new Mock<IFlatFileService>();
        styleFlatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });
        var styleTable = new DataTable();
        styleTable.Columns.Add(ADDataFeedSourceFile.NumericCodeColumn, typeof(string));
        styleTable.Columns.Add($"{ADDataFeedSourceFile.NodeNameColumn}", typeof(string));
        styleTable.Columns.Add(
            $"{ADDataFeedSourceFile.CategoryAttributeNameColumn}",
            typeof(string)
        );
        styleTable.Columns.Add($"{ADDataFeedSourceFile.DisplaySequenceColumn}", typeof(string));
        styleTable.Rows.Add("3713310000", "Coated Abrasive Belts", "Abrasive Material", "1");
        styleFlatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(styleTable);
        var integrationProcessADDataFeed = new TestableIntegrationProcessorADDataFeed(
            styleFlatFileService.Object
        );

        var siteConnection = this.MockConnection();

        var styledataSet = integrationProcessADDataFeed.Execute(
            siteConnection,
            GetIntegrationJob(
                sourceFolderParameterValue: "files/adtest",
                documentFolderParameterValue: "path/to/documents"
            ),
            this.MockJobDefinitionStep("styleclass")
        );
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            (ADDataFeedSourceFile.ProductNameColumn, null),
            (ADDataFeedSourceFile.CategoryCodeColumn, null),
            (ADDataFeedSourceFile.ProductGroupColumn, null),
            ($"{ADDataFeedSourceFile.ItemDocumentNameColumn}_1", "http://localhost/manual.pdf"),
            ($"{ADDataFeedSourceFile.ItemDocumentTypeColumn}_1", null),
            ($"{ADDataFeedSourceFile.DocCaptionColumn}_1", null)
        );
        styleFlatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = integrationProcessADDataFeed.Execute(
            siteConnection,
            GetIntegrationJob(
                sourceFolderParameterValue: "files/adtest",
                documentFolderParameterValue: "path/to/documents"
            ),
            this.MockJobDefinitionStep()
        );

        var documentTable = dataSet.Tables["11Document"];
        documentTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[]
                    {
                        "SAW123",
                        "Product",
                        "http://localhost/manual.pdf",
                        string.Empty,
                        string.Empty
                    }
                }
            );
    }

    [Test]
    public void Execute_Should_Not_Map_Paths_For_Documents_That_Use_Full_Url_For_Enterworks_Data_File_Format()
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService.Setup(o => o.GetFiles()).Returns(new List<string> { "dataFile" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            (
                $"{ADDataFeedSourceFile.EnterworksItemDocumentNameColumn}_1",
                "http://localhost/manual.pdf"
            ),
            ($"{ADDataFeedSourceFile.EnterworksDocCaptionColumn}_1", "Power saw manual")
        );
        flatFileService.Setup(o => o.ParseFile(It.IsAny<string>())).Returns(table);

        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            GetIntegrationJob(
                sourceFolderParameterValue: "files/adtest",
                documentFolderParameterValue: "path/to/documents"
            ),
            this.MockJobDefinitionStep()
        );

        var documentTable = dataSet.Tables["11Document"];
        documentTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[]
                    {
                        "SAW123",
                        "Product",
                        "http://localhost/manual.pdf",
                        string.Empty,
                        "Power saw manual"
                    }
                }
            );
    }

    [TestCase(
        "Member_Part_Number",
        "CATEGORY_CODE_1_LEVEL_1",
        "CATEGORY_NAME_1_LEVEL_1",
        "CATEGORY_CODE_1_LEVEL_2",
        "CATEGORY_NAME_1_LEVEL_2"
    )]
    // This AD data file format starts with category column names that don't have a number suffix.
    [TestCase(
        "Member Part Number",
        "CATEGORY_CODE_LEVEL_1",
        "CATEGORY_NAME_LEVEL_1",
        "CATEGORY_CODE_LEVEL_2",
        "CATEGORY_NAME_LEVEL_2"
    )]
    public void Execute_Should_Have_Record_For_Each_Unique_Category_In_Category_Table(
        string partNumberColumn,
        string categoryCodeLevel1Column,
        string categoryNameLevel1Column,
        string categoryCodeLevel2Column,
        string categoryNameLevel2Column
    )
    {
        var rawData = this.MockProducts(
            (partNumberColumn, "WRENCH-1234"),
            (categoryCodeLevel1Column, "HANDTOOLS"),
            (categoryNameLevel1Column, "Hand Tools"),
            (categoryCodeLevel2Column, "WRENCHES"),
            (categoryNameLevel2Column, "Wrenches")
        );
        var processor = ConfigureProcessor(new (string, DataTable)[] { ("AD.csv", rawData) });

        var dataSet = processor.Execute(
            this.MockConnection(),
            this.MockJob(),
            this.MockJobDefinitionStep()
        );

        var processedCategories = dataSet.Tables["12Category"];
        processedCategories
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[] { "ACME:HANDTOOLS", "HANDTOOLS", "Hand Tools" },
                    new object[] { "ACME:HANDTOOLS:WRENCHES", "WRENCHES", "Wrenches" },
                }
            );
    }

    [TestCase(
        "Member_Part_Number",
        "CATEGORY_CODE_1_LEVEL_1",
        "CATEGORY_NAME_1_LEVEL_1",
        "CATEGORY_CODE_1_LEVEL_2",
        "CATEGORY_NAME_1_LEVEL_2"
    )]
    // This AD data file format starts with category column names that don't have a number suffix.
    [TestCase(
        "Member Part Number",
        "CATEGORY_CODE_LEVEL_1",
        "CATEGORY_NAME_LEVEL_1",
        "CATEGORY_CODE_LEVEL_2",
        "CATEGORY_NAME_LEVEL_2"
    )]
    public void Execute_Should_Have_Record_For_Each_Unique_Category_And_Product_In_Product_Table(
        string partNumberColumn,
        string categoryCodeLevel1Column,
        string categoryNameLevel1Column,
        string categoryCodeLevel2Column,
        string categoryNameLevel2Column
    )
    {
        var rawData = this.MockProducts(
            (partNumberColumn, "WRENCH-1234"),
            (categoryCodeLevel1Column, "HANDTOOLS"),
            (categoryNameLevel1Column, "Hand Tools"),
            (categoryCodeLevel2Column, "WRENCHES"),
            (categoryNameLevel2Column, "Wrenches")
        );

        var processor = ConfigureProcessor(new (string, DataTable)[] { ("AD.csv", rawData) });

        var dataSet = processor.Execute(
            this.MockConnection(),
            this.MockJob(),
            this.MockJobDefinitionStep()
        );

        var processedCategories = dataSet.Tables["13CategoryProduct"];
        foreach (DataRow row in processedCategories.Rows)
        {
            row.Should()
                .HaveColumns("WebsiteCategoryName", ADDataFeedSourceFile.MyPartNumberColumn);
        }
    }

    [TestCase(
        "Member_Part_Number",
        "CATEGORY_CODE_1_LEVEL_1",
        "CATEGORY_NAME_1_LEVEL_1",
        "CATEGORY_CODE_1_LEVEL_2",
        "CATEGORY_NAME_1_LEVEL_2",
        "CATEGORY_CODE_1_LEVEL_3",
        "CATEGORY_NAME_1_LEVEL_3"
    )]
    // This AD data file format starts with category column names that don't have a number suffix.
    [TestCase(
        "Member Part Number",
        "CATEGORY_CODE_LEVEL_1",
        "CATEGORY_NAME_LEVEL_1",
        "CATEGORY_CODE_LEVEL_2",
        "CATEGORY_NAME_LEVEL_2",
        "CATEGORY_CODE_LEVEL_3",
        "CATEGORY_NAME_LEVEL_3"
    )]
    public void Execute_Should_Not_Have_Blank_Sub_Category_Codes(
        string partNumberColumn,
        string categoryCodeLevel1Column,
        string categoryNameLevel1Column,
        string categoryCodeLevel2Column,
        string categoryNameLevel2Column,
        string categoryCodeLevel3Column,
        string categoryNameLevel3Column
    )
    {
        var rawData = this.MockProducts(
            (partNumberColumn, "WRENCH-1234"),
            (categoryCodeLevel1Column, "HANDTOOLS"),
            (categoryNameLevel1Column, "Hand Tools"),
            (categoryCodeLevel2Column, "WRENCHES"),
            (categoryNameLevel2Column, "Wrenches"),
            (categoryCodeLevel3Column, string.Empty), // Note that this category level is ignored due to the empty code value
            (categoryNameLevel3Column, "Steel")
        );
        var processor = ConfigureProcessor(new (string, DataTable)[] { ("AD.csv", rawData) });

        var dataSet = processor.Execute(
            this.MockConnection(),
            this.MockJob(),
            this.MockJobDefinitionStep()
        );

        var processedCategories = dataSet.Tables["12Category"];
        processedCategories
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[] { "ACME:HANDTOOLS", "HANDTOOLS", "Hand Tools" },
                    new object[] { "ACME:HANDTOOLS:WRENCHES", "WRENCHES", "Wrenches" },
                }
            );
    }

    [Test]
    public void Execute_Should_Create_Attribute_Type_And_Value_Translation_Tables()
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService
            .Setup(o => o.GetFiles())
            .Returns(new List<string> { "translation", "base" });

        var table = this.MockProducts(
            (ADDataFeedSourceFile.MyPartNumberColumn, "SAW123"),
            ($"{ADDataFeedSourceFile.AttributeNameColumn}_1", "ANSI Code FR"),
            ($"{ADDataFeedSourceFile.AttributeValueUomColumn}_1", "Type IA FR")
        );

        flatFileService.Setup(o => o.ParseFile("translation.processed")).Returns(table);
        flatFileService
            .Setup(o => o.MoveFileToProcessing("translation"))
            .Returns("translation.processed");

        var table2 = new DataTable();
        table2.Columns.Add("Attribute Name 1", typeof(string));
        table2.Columns.Add("Value UOM 1", typeof(string));

        table2.Rows.Add("ANSI Code", "Type IA");
        table2.Rows.Add("ANSI Code", "Type IA");
        flatFileService.Setup(o => o.ParseFile("base.processed")).Returns(table2);
        flatFileService.Setup(o => o.MoveFileToProcessing("base")).Returns("base.processed");

        var job = GetIntegrationJob(
            websiteParameterValue: "TomsTrunk",
            languageCode: "FR",
            baseLanguageFileName: "base"
        );
        var definitionStep = this.MockJobDefinitionStep();
        definitionStep.Name = "AD Data Feed Translations Refresh";
        var dataSet = new TestableIntegrationProcessorADDataFeed(flatFileService.Object).Execute(
            this.MockConnection(),
            job,
            definitionStep
        );

        var attributeValueTable = dataSet.Tables["7TranslationDictionary"];
        attributeValueTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[] { "0", "AttributeValue", "Type IA", "Type IA FR", "FR" }
                }
            );

        var attributeTypeTable = dataSet.Tables["6TranslationDictionary"];
        attributeTypeTable
            .Should()
            .ContainRowsInOrderWithValues(
                new List<object[]>
                {
                    new object[] { "0", "Attribute", "ANSI Code", "ANSI Code FR", "FR" }
                }
            );
    }

    private static TestableIntegrationProcessorADDataFeed ConfigureProcessor(
        IEnumerable<(string FileName, DataTable DataTable)> fileNamesToDataTables
    )
    {
        var flatFileService = new Mock<IFlatFileService>();
        flatFileService
            .Setup(o => o.GetFiles())
            .Returns(fileNamesToDataTables.Select(tuple => tuple.FileName));
        foreach (var (fileName, dataTable) in fileNamesToDataTables)
        {
            flatFileService
                .Setup(o => o.MoveFileToProcessing(fileName))
                .Returns("processing_" + fileName);
            flatFileService.Setup(o => o.ParseFile("processing_" + fileName)).Returns(dataTable);
        }

        return new TestableIntegrationProcessorADDataFeed(flatFileService.Object);
    }

    internal class TestableIntegrationProcessorADDataFeed : IntegrationProcessorADDataFeed
    {
        private readonly IFlatFileService flatFileService;

        public TestableIntegrationProcessorADDataFeed(IFlatFileService flatFileService)
        {
            this.flatFileService = flatFileService;
        }

        protected override IFlatFileService GetFlatFileService(
            IntegrationJob integrationJob,
            JobDefinitionStep jobDefinitionStep
        ) => this.flatFileService;
    }

    private static IntegrationJob GetIntegrationJob(
        string websiteParameterValue = "",
        string excludedAttributesParameterValue = "",
        string imagesParameterValue = null,
        string sourceFolderParameterValue = null,
        string smallFolderParameterValue = null,
        string mediumFolderParameterValue = null,
        string largeFolderParameterValue = null,
        string documentFolderParameterValue = null,
        string languageCode = "",
        string baseLanguageFileName = null
    )
    {
        return new IntegrationJob
        {
            IntegrationJobParameters = new IntegrationJobParameter[]
            {
                new ADTestIntegrationJobParameter(websiteParameterValue, "Website"),
                new ADTestIntegrationJobParameter(
                    excludedAttributesParameterValue,
                    "ExcludedAttributes"
                ),
                new ADTestIntegrationJobParameter(imagesParameterValue, "Images"),
                new ADTestIntegrationJobParameter(sourceFolderParameterValue, "SourceFolder"),
                new ADTestIntegrationJobParameter(smallFolderParameterValue, "SmallFolder"),
                new ADTestIntegrationJobParameter(mediumFolderParameterValue, "MediumFolder"),
                new ADTestIntegrationJobParameter(largeFolderParameterValue, "LargeFolder"),
                new ADTestIntegrationJobParameter(documentFolderParameterValue, "DocumentFolder"),
                new ADTestIntegrationJobParameter(languageCode, "LanguageCode"),
                new ADTestIntegrationJobParameter(baseLanguageFileName, "BaseLanguageFileName")
            },
            JobDefinition = new JobDefinition
            {
                IntegrationConnection = new IntegrationConnection()
            }
        };
    }

    internal SiteConnection MockConnection()
    {
        var mockConnection = new Mock<SiteConnection>();
        mockConnection.Object.UserName = "admin";
        mockConnection.Object.Password = "admin";
        mockConnection
            .Setup(
                o =>
                    o.AddLogMessage(
                        It.IsAny<string>(),
                        It.IsAny<IntegrationJobLogType>(),
                        It.IsAny<string>()
                    )
            )
            .Returns(true);

        return mockConnection.Object;
    }

    internal JobDefinitionStep MockJobDefinitionStep(
        string objectName = "product",
        JobDefinitionStepParameter[] parameters = null,
        JobDefinitionStepFieldMap[] extraSteps = null
    )
    {
        var mockStep = new Mock<JobDefinitionStep>();
        var mockFieldMapStep1 = new Mock<JobDefinitionStepFieldMap>();
        var mockFieldMapStep2 = new Mock<JobDefinitionStepFieldMap>();
        var jobParameters = new List<JobDefinitionStepParameter>();
        var fieldMapSteps = new List<JobDefinitionStepFieldMap>
        {
            mockFieldMapStep1.Object,
            mockFieldMapStep2.Object
        };

        mockStep.Object.ObjectName = objectName;
        mockStep.Object.Name = "AD Data Feed Refresh";

        mockFieldMapStep1.Object.ToProperty = "ShortDescription";
        mockFieldMapStep1.Object.FromProperty = ADDataFeedSourceFile.SkuShortDescriptionColumn;
        mockFieldMapStep1.Object.FieldType = "Field";

        mockFieldMapStep2.Object.ToProperty = "ContentManager";
        mockFieldMapStep2.Object.FromProperty = ADDataFeedSourceFile.MarketingDescriptionColumn;
        mockFieldMapStep2.Object.FieldType = "Field";

        if (parameters != null)
        {
            jobParameters.AddRange(parameters);
        }

        if (extraSteps != null)
        {
            fieldMapSteps.AddRange(extraSteps);
        }

        mockStep.Object.JobDefinitionStepFieldMaps = fieldMapSteps.ToArray();
        mockStep.Object.JobDefinitionStepParameters = parameters;
        return mockStep.Object;
    }

    internal IntegrationJob MockJob(IntegrationJobParameter[] jobParameters = null)
    {
        var mockJob = new Mock<IntegrationJob>();
        var mockDefinition = new Mock<JobDefinition>();
        var mockIntegrationConnection = new Mock<IntegrationConnection>();
        var mockIntegrationJobParameter = new Mock<IntegrationJobParameter>();
        var mockJobDefinitionParameter = new Mock<JobDefinitionParameter>();

        mockIntegrationJobParameter.Object.Value = "ACME";
        mockIntegrationJobParameter.Object.JobDefinitionParameter =
            mockJobDefinitionParameter.Object;

        var integrationJobParameters = new List<IntegrationJobParameter>
        {
            mockIntegrationJobParameter.Object
        };

        if (jobParameters != null)
        {
            integrationJobParameters.AddRange(jobParameters);
        }

        mockJobDefinitionParameter.Object.Name = "Website";
        mockJob.Object.IntegrationJobParameters = integrationJobParameters.ToArray();

        mockIntegrationConnection.Object.DebuggingEnabled = false;
        mockDefinition.Object.IntegrationConnection = mockIntegrationConnection.Object;
        mockJob.Object.JobDefinition = mockDefinition.Object;

        return mockJob.Object;
    }

    internal DataTable MockProducts(params (string, string)[] optionalColumns)
    {
        var table = new DataTable("4product");

        table.Columns.AddRange(
            new DataColumn[]
            {
                new DataColumn(ADDataFeedSourceFile.MyPartNumberColumn, typeof(string)),
                new DataColumn(ADDataFeedSourceFile.SkuShortDescriptionColumn, typeof(string)),
                new DataColumn(ADDataFeedSourceFile.SKULongDescription1Column, typeof(string)),
                new DataColumn(ADDataFeedSourceFile.SKULongDescription2Column, typeof(string)),
                new DataColumn(ADDataFeedSourceFile.BrandNameColumn, typeof(string)),
                new DataColumn(ADDataFeedSourceFile.EANColumn, typeof(string)),
                new DataColumn(ADDataFeedSourceFile.MarketingDescriptionColumn, typeof(string)),
                new DataColumn(ADDataFeedSourceFile.UPCColumn, typeof(string)),
            }
        );

        var columnList = new List<DataColumn>();
        foreach (var pair in optionalColumns)
        {
            if (!string.Equals(pair.Item1, ADDataFeedSourceFile.MyPartNumberColumn))
            {
                columnList.Add(new DataColumn(pair.Item1, typeof(string)));
            }
        }

        table.Columns.AddRange(columnList.ToArray());

        var wrenchProduct = table.NewRow();
        var hammerProduct = table.NewRow();

        wrenchProduct[ADDataFeedSourceFile.MyPartNumberColumn] = "Test";
        wrenchProduct[ADDataFeedSourceFile.SkuShortDescriptionColumn] = "Example";
        wrenchProduct[ADDataFeedSourceFile.SKULongDescription1Column] = "Example Long 1";
        wrenchProduct[ADDataFeedSourceFile.SKULongDescription2Column] = "Example Long 2";
        wrenchProduct[ADDataFeedSourceFile.BrandNameColumn] = "Minneapolis";
        wrenchProduct[ADDataFeedSourceFile.EANColumn] = "12345";
        wrenchProduct[ADDataFeedSourceFile.MarketingDescriptionColumn] = "Example";
        wrenchProduct[ADDataFeedSourceFile.UPCColumn] = "54321";

        hammerProduct[ADDataFeedSourceFile.MyPartNumberColumn] = "Test";
        hammerProduct[ADDataFeedSourceFile.SkuShortDescriptionColumn] = "Example";
        hammerProduct[ADDataFeedSourceFile.SKULongDescription1Column] = "Example Long 1";
        hammerProduct[ADDataFeedSourceFile.SKULongDescription2Column] = "Example Long 2";
        hammerProduct[ADDataFeedSourceFile.BrandNameColumn] = "Minneapolis";
        hammerProduct[ADDataFeedSourceFile.EANColumn] = "12345";
        hammerProduct[ADDataFeedSourceFile.MarketingDescriptionColumn] = "Example";
        hammerProduct[ADDataFeedSourceFile.UPCColumn] = "54321";

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

internal class ADTestIntegrationJobParameter : IntegrationJobParameter
{
    public ADTestIntegrationJobParameter(params string[] args)
    {
        this.Value = args[0];
        this.JobDefinitionParameter = new JobDefinitionParameter
        {
            Name = args[1],
            DefaultValue = string.Empty,
        };
        this.JobDefinitionStepParameter = new JobDefinitionStepParameter { Name = args[1] };
    }
}
