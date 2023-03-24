namespace Insite.WIS.InRiver.Tests;

using System;
using System.Collections.Generic;
using System.Data;

using Insite.WIS.Broker;
using Insite.WIS.Broker.WebIntegrationService;
using Insite.WIS.InRiver;
using Insite.WIS.InRiver.Models;

using NUnit.Framework;
using TranslationDictionary = Insite.WIS.InRiver.Models.TranslationDictionary;

[TestFixture]
public class IntegrationProcessorInRiverChannelNodeTests
{
    [SetUp]
    public void Setup()
    {
        this.integrationProcessorInRiverChannelNode =
            new TestableIntegrationProcessorInRiverChannelNode(null);
    }

    private TestableIntegrationProcessorInRiverChannelNode integrationProcessorInRiverChannelNode;

    [Test]
    public void CategoryName_Should_Return_Emtpy_When_No_Channel_Nodes()
    {
        var actual = this.integrationProcessorInRiverChannelNode.CallCategoryName(
            new InRiverGenericObject()
            {
                Fields = new Dictionary<string, IEnumerable<TranslationDictionary>>()
            },
            "EN"
        );

        Assert.AreEqual(string.Empty, actual);
    }

    [Test]
    public void CategoryName_Should_Return_Category_Name()
    {
        var languageCode = "EN";
        var expected = "MyCategoryName";
        var channelNode = CreateChannelNode(languageCode, expected);

        var actual = this.integrationProcessorInRiverChannelNode.CallCategoryName(
            channelNode,
            "EN"
        );

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CategoryName_Should_Return_Category_Name_With_Language_Culture()
    {
        var languageCode = "EN";
        var expected = "MyCategoryName";
        var channelNode = CreateChannelNode(languageCode, expected);

        var actual = this.integrationProcessorInRiverChannelNode.CallCategoryName(
            channelNode,
            "EN-US"
        );

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void CategoryName_Should_Return_Empty_When_Not_Found()
    {
        var languageCode = "EN";
        var expected = string.Empty;
        var channelNode = CreateChannelNode(languageCode, expected);

        var actual = this.integrationProcessorInRiverChannelNode.CallCategoryName(
            channelNode,
            "SP-US"
        );

        Assert.AreEqual(expected, actual);
    }

    private static InRiverGenericObject CreateChannelNode(string languageCode, string expected)
    {
        var channelNode = new InRiverGenericObject
        {
            Fields = new Dictionary<string, IEnumerable<TranslationDictionary>>()
        };
        var field = new List<TranslationDictionary>();
        var fieldTranslation = new TranslationDictionary
        {
            LanguageCode = languageCode,
            TranslatedValue = expected
        };
        field.Add(fieldTranslation);
        channelNode.Fields.Add("ChannelNodeName", field);
        return channelNode;
    }
}

public class TestableIntegrationProcessorInRiverChannelNode
    : IntegrationProcessorInRiverChannelNodeToProduct
{
    public TestableIntegrationProcessorInRiverChannelNode(IntegrationProcessorFileHelper fileHelper)
        : base(fileHelper) { }

    public string CallCategoryName(InRiverGenericObject channelNode, string defaultLanguage)
    {
        return this.CategoryName(channelNode, defaultLanguage);
    }

    public override DataTable ProcessInRiverCollection(
        IEnumerable<InRiverGenericObject> inRiverCollection,
        DataTable dataTableSchema,
        IntegrationJob integrationJob,
        SiteConnection siteConnection,
        JobDefinitionStep integrationJobStep
    )
    {
        throw new NotImplementedException();
    }
}
