namespace MessageBroker.Common;

public static class MessageValidator
{
    public static void EnsureValid(BrokerMessage message, string? expectedRecipient = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (string.IsNullOrWhiteSpace(message.MessageId))
            throw new InvalidDataException("Lipsește identificatorul mesajului.");

        if (string.IsNullOrWhiteSpace(message.SenderId))
            throw new InvalidDataException("Lipsește identificatorul expeditorului.");

        if (string.IsNullOrWhiteSpace(message.RecipientId))
            throw new InvalidDataException("Lipsește identificatorul destinatarului.");

        if (string.IsNullOrWhiteSpace(message.MessageType))
            throw new InvalidDataException("Lipsește tipul mesajului.");

        if (message.Payload is null)
            throw new InvalidDataException("Conținutul mesajului nu poate fi null.");

        if (message.CreatedAtUtc == default)
            throw new InvalidDataException("Data creării mesajului este invalidă.");

        if (expectedRecipient is not null &&
            !string.Equals(message.RecipientId, expectedRecipient, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"Mesajul este adresat către '{message.RecipientId}', dar coada aparține lui '{expectedRecipient}'.");
        }
    }

    public static void EnsureValidIdentifier(string identifier, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("Identificatorul nu poate fi gol.", parameterName);

        if (identifier.Length > 40 || identifier.Any(c =>
                !(char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-')))
        {
            throw new ArgumentException(
                "Folosește maximum 40 de caractere: litere ASCII, cifre, punct, underscore sau cratimă.",
                parameterName);
        }
    }
}
