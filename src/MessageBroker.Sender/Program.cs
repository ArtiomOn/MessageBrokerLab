using MessageBroker.Common;
using MessageBroker.Sender;

Console.OutputEncoding = System.Text.Encoding.UTF8;

try
{
    await using var publisher = await MessagePublisher.CreateAsync();

    Console.WriteLine("=== MESSAGE BROKER - SENDER ===");
    Console.WriteLine("Conectat la RabbitMQ. Alege operația:");

    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("1. Trimite mesaj direct către un Receiver");
        Console.WriteLine("2. Publică un eveniment către toți abonații (Publish-Subscribe)");
        Console.WriteLine("3. Trimite XML invalid (test Dead Letter Queue)");
        Console.WriteLine("Q. Ieșire");
        Console.Write("Opțiune: ");

        var option = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (option is "q" or "quit" or "exit")
            break;

        try
        {
            switch (option)
            {
                case "1":
                {
                    Console.Write("ID-ul destinatarului (ex. user1): ");
                    var receiverId = Console.ReadLine()?.Trim() ?? string.Empty;
                    MessageValidator.EnsureValidIdentifier(receiverId, "receiverId");

                    Console.Write("Mesaj: ");
                    var payload = Console.ReadLine() ?? string.Empty;

                    var message = CreateMessage(receiverId, payload);
                    await publisher.PublishDirectAsync(message);
                    break;
                }
                case "2":
                {
                    Console.Write("Eveniment: ");
                    var payload = Console.ReadLine() ?? string.Empty;

                    var message = CreateMessage("all-subscribers", payload);
                    await publisher.PublishEventAsync(message);
                    break;
                }
                case "3":
                {
                    Console.Write("ID-ul destinatarului care ascultă (ex. user1): ");
                    var receiverId = Console.ReadLine()?.Trim() ?? string.Empty;
                    await publisher.PublishInvalidXmlAsync(receiverId);
                    break;
                }
                default:
                    Console.WriteLine("Opțiune necunoscută.");
                    break;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"[EROARE] {exception.Message}");
        }
    }
}
catch (Exception exception)
{
    Console.Error.WriteLine("Nu m-am putut conecta la RabbitMQ.");
    Console.Error.WriteLine(exception.Message);
    Console.Error.WriteLine("Verifică dacă RabbitMQ rulează și dacă setările de conexiune sunt corecte.");
    Environment.ExitCode = 1;
}

static BrokerMessage CreateMessage(string recipientId, string payload) => new()
{
    MessageId = Guid.NewGuid().ToString("N"),
    SenderId = "sender-console",
    RecipientId = recipientId,
    MessageType = "text",
    Payload = payload,
    CreatedAtUtc = DateTime.UtcNow
};
