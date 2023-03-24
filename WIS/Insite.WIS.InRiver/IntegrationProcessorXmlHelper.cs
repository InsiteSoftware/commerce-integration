namespace Insite.WIS.InRiver;

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using Insite.WIS.InRiver.Models;

/// <summary>The integration processor xml helper.</summary>
public static class IntegrationProcessorXmlHelper
{
    public static string GetAttributeValue(string attributeName, XElement element)
    {
        return element.Attribute(attributeName) == null
            ? string.Empty
            : element.Attribute(attributeName).Value;
    }

    public static string GetNodeValue(string nodeName, XElement xmlNode, XNamespace xmlNamespace)
    {
        var node = xmlNode.Descendants().SingleOrDefault(o => o.Name.LocalName.Equals(nodeName));
        return node != null ? node.Value : string.Empty;
    }

    public static IEnumerable<TranslationDictionary> GetTranslatedFieldValue(
        XElement dataElement,
        XNamespace xmlNamespace,
        string defaultLanguage
    )
    {
        var translationDictionary = new List<TranslationDictionary>();

        var localeNodes = dataElement.Elements(xmlNamespace + "LocaleString").ToList();
        if (localeNodes.Any())
        {
            foreach (var localeNode in localeNodes)
            {
                var translationEntry = new TranslationDictionary();
                var languageElement = localeNode.Element(xmlNamespace + "Language");
                if (languageElement != null)
                {
                    translationEntry.LanguageCode = languageElement.Value;
                }

                var valueElement = localeNode.Element(xmlNamespace + "Value");
                if (valueElement != null)
                {
                    translationEntry.TranslatedValue = valueElement.Value;
                }

                translationDictionary.Add(translationEntry);
            }
        }
        else
        {
            var translationEntry = new TranslationDictionary
            {
                LanguageCode = defaultLanguage,
                TranslatedValue = dataElement.Value
            };

            translationDictionary.Add(translationEntry);
        }

        return translationDictionary;
    }

    public static string GetUniqueIdentifier(XElement xmlNode, XNamespace xmlNamespace)
    {
        var entityId = GetAttributeValue("EntityId", xmlNode);
        var uniqueIdField = GetAttributeValue("ExternalUniqueIdField", xmlNode);
        if (!string.IsNullOrEmpty(uniqueIdField))
        {
            var uniqueId = GetNodeValue(uniqueIdField, xmlNode, xmlNamespace);
            if (!string.IsNullOrEmpty(uniqueIdField))
            {
                return uniqueId;
            }
        }

        return entityId;
    }

    public static void ProcessChildrenNodes(
        XElement linksNode,
        XNamespace xmlNamespace,
        string objectName,
        InRiverGenericObject inRiverObject
    )
    {
        var childrenLinkNode = linksNode.Element(xmlNamespace + objectName + "ChildLinks");
        if (childrenLinkNode == null)
        {
            return;
        }

        foreach (var childNode in childrenLinkNode.Elements())
        {
            var child = new InRiverGenericObject
            {
                UniqueIdentifier = GetAttributeValue("UniqueValue", childNode),
                EntityType = GetAttributeValue("TargetEntityTypeId", childNode),
                Action = GetAttributeValue("Action", childNode)
            };

            inRiverObject.Children.Add(child);
        }
    }

    public static void ProcessParentNode(
        XElement linksNode,
        XNamespace xmlNamespace,
        string objectName,
        InRiverGenericObject inRiverObject
    )
    {
        var parentLinkNode = linksNode.Element(xmlNamespace + objectName + "ParentLinks");
        if (parentLinkNode == null)
        {
            return;
        }

        var parentLinkNodes = parentLinkNode.Elements();
        foreach (var parentLink in parentLinkNodes)
        {
            if (
                parentLink != null
                && !string.IsNullOrEmpty(GetAttributeValue("UniqueValue", parentLink))
            )
            {
                var parentObject = new InRiverGenericObject
                {
                    UniqueIdentifier = GetAttributeValue("UniqueValue", parentLink)
                };
                inRiverObject.Parents.Add(parentObject);
            }
        }
    }

    public static void ProcessFields(
        XElement node,
        XNamespace xmlNamespace,
        string objectName,
        string defaultLanguage,
        InRiverGenericObject inRiverObject
    )
    {
        var fieldNodes = node.Element(xmlNamespace + objectName + "Fields");
        if (fieldNodes == null)
        {
            return;
        }

        var fields = fieldNodes.Elements();
        foreach (var field in fields)
        {
            var dataElement = field.Element(xmlNamespace + "Data");
            if (dataElement != null)
            {
                inRiverObject.Fields.Add(
                    field.Name.LocalName,
                    GetTranslatedFieldValue(dataElement, xmlNamespace, defaultLanguage)
                );
            }
        }
    }
}
