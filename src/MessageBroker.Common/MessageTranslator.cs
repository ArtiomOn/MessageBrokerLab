using System.Text.Json;
using System.Text.Json.Serialization;

namespace MessageBroker.Common;

public static class MessageTranslator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string ToJson(BrokerMessage message) =>
        JsonSerializer.Serialize(message, JsonOptions);
}
