using System.Xml.Serialization;

namespace MessageBroker.Common;

[XmlRoot("message")]
public sealed class BrokerMessage
{
    [XmlElement("messageId")]
    public string MessageId { get; set; } = Guid.NewGuid().ToString("N");

    [XmlElement("senderId")]
    public string SenderId { get; set; } = "sender-console";

    [XmlElement("recipientId")]
    public string RecipientId { get; set; } = string.Empty;

    [XmlElement("messageType")]
    public string MessageType { get; set; } = "text";

    [XmlElement("payload")]
    public string Payload { get; set; } = string.Empty;

    [XmlElement("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
