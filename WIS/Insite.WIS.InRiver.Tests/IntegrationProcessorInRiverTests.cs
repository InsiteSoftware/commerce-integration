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
public class IntegrationProcessorInRiverTests
{
    [SetUp]
    public void Setup()
    {
        this.integrationProcessorInRiver = new TestableIntegrationProcessorInRiver(null);
    }

    private TestableIntegrationProcessorInRiver integrationProcessorInRiver;

    private string GetTranslatedFieldValue(
        FieldBuilder fields,
        string fieldName,
        string languageCode
    )
    {
        return this.integrationProcessorInRiver.CallGetTranslatedFieldValue(
            fields.Build(),
            fieldName,
            languageCode
        );
    }

    private static FieldBuilder ASetOfFields()
    {
        return new FieldBuilder();
    }

    [Test]
    public void GetTranslatedFieldValue_Should_Translate_Field()
    {
        var fieldName = "MyTestField";
        var languageCode = "EN-US";
        var expected = "MyTestFieldInEnglish";
        var fields = ASetOfFields().WithField(fieldName, languageCode, expected);

        var actual = this.GetTranslatedFieldValue(fields, fieldName, languageCode);

        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void GetTranslatedFieldValue_Should_Translate_Field_Without_Culture()
    {
        var fieldName = "MyTestField";
        var languageCode = "EN";
        var expected = "MyTestFieldInEnglish";
        var fields = ASetOfFields().WithField(fieldName, languageCode, expected);

        var actual = this.GetTranslatedFieldValue(fields, fieldName, "EN-US");

        Assert.AreEqual(expected, actual);
    }
}

internal class FieldBuilder
{
    private readonly Dictionary<string, IEnumerable<TranslationDictionary>> fields =
        new Dictionary<string, IEnumerable<TranslationDictionary>>();

    public FieldBuilder WithField(string fieldName, string languageCode, string value)
    {
        var fieldTranslations = new[]
        {
            new TranslationDictionary
            {
                LanguageCode = languageCode,
                TranslatedValue = "MyTestFieldInEnglish"
            }
        };

        this.fields.Add(fieldName, fieldTranslations);

        return this;
    }

    public IDictionary<string, IEnumerable<TranslationDictionary>> Build()
    {
        return this.fields;
    }
}

public class TestableIntegrationProcessorInRiver : IntegrationProcessorInRiver
{
    public TestableIntegrationProcessorInRiver(IntegrationProcessorFileHelper fileHelper)
        : base(fileHelper) { }

    public string CallGetTranslatedFieldValue(
        IDictionary<string, IEnumerable<TranslationDictionary>> fields,
        string fieldName,
        string languageCode
    )
    {
        return this.GetTranslatedFieldValue(fields, fieldName, languageCode);
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
