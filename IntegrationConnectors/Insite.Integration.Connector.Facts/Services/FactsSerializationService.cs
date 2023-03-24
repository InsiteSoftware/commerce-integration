namespace Insite.Integration.Connector.Facts.Services;

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

internal static class FactsSerializationService
{
    public static string Serialize<T>(T request)
    {
        try
        {
            using (var stringwriter = new StringWriter())
            {
                var serializer = new XmlSerializer(typeof(T));

                serializer.Serialize(stringwriter, request);

                return stringwriter.ToString();
            }
        }
        catch (Exception exception)
        {
            throw new Exception(
                $"Error serializing {typeof(T).FullName} to XML request. Message: {exception.Message}."
            );
        }
    }

    public static T Deserialize<T>(string response)
    {
        try
        {
            using (var stringReader = new StringReader(response))
            using (var xmlTextReader = new NamespaceIgnorantXmlTextReader(stringReader))
            {
                var serializer = new XmlSerializer(typeof(T));

                return (T)serializer.Deserialize(xmlTextReader);
            }
        }
        catch (Exception exception)
        {
            throw new Exception(
                $"Error deserializing XML response to {typeof(T).FullName}. XML Response: {response}. Message: {exception.Message}."
            );
        }
    }

    private class NamespaceIgnorantXmlTextReader : XmlTextReader
    {
        public NamespaceIgnorantXmlTextReader(TextReader reader)
            : base(reader) { }

        public override string NamespaceURI
        {
            get { return string.Empty; }
        }
    }
}
