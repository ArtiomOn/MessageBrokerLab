using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace MessageBroker.Common;

public static class MessageXmlSerializer
{
    public static string Serialize(BrokerMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var serializer = new XmlSerializer(typeof(BrokerMessage));
        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true,
            OmitXmlDeclaration = false
        };

        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, settings))
        {
            serializer.Serialize(writer, message);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static BrokerMessage Deserialize(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        var serializer = new XmlSerializer(typeof(BrokerMessage));
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreWhitespace = true
        };

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, settings);

        return serializer.Deserialize(xmlReader) as BrokerMessage
               ?? throw new InvalidDataException("Mesajul XML nu conține un obiect BrokerMessage valid.");
    }
}
