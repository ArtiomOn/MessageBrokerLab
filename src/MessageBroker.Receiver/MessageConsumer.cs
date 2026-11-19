using System.Text;
using MessageBroker.Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBroker.Receiver;

public sealed class MessageConsumer
{
    public async Task RunReceiverAsync(
        string receiverId,
        bool subscribeToEvents,
        CancellationToken cancellationToken)
    {
        MessageValidator.EnsureValidIdentifier(receiverId, nameof(receiverId));

        var factory = RabbitMqConnectionFactory.Create($"message-broker-receiver-{receiverId}");
        await using var connection = await factory.CreateConnectionAsync();
        await using var directChannel = await connection.CreateChannelAsync();

        var directQueue = await RabbitMqTopology.DeclareDirectReceiverAsync(directChannel, receiverId);
        await StartQueueConsumerAsync(
            directChannel,
            directQueue,
            expectedRecipient: receiverId,
            displayName: $"DIRECT / {receiverId}");

        IChannel? eventChannel = null;
        try
        {
            if (subscribeToEvents)
            {
                eventChannel = await connection.CreateChannelAsync();
                var eventQueue = await RabbitMqTopology.DeclareSubscriberAsync(eventChannel, receiverId);
                await StartQueueConsumerAsync(
                    eventChannel,
                    eventQueue,
                    expectedRecipient: "all-subscribers",
                    displayName: $"PUBLISH-SUBSCRIBE / {receiverId}");
            }

            Console.WriteLine();
            Console.WriteLine($"[RECEIVER] Ascult coada directă: {directQueue}");
            if (subscribeToEvents)
                Console.WriteLine($"[RECEIVER] Sunt abonat și la evenimente. Coada: {BrokerConstants.GetSubscriberQueueName(receiverId)}");
            Console.WriteLine("Apasă Ctrl+C pentru oprire.");

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        finally
        {
            if (eventChannel is not null)
                await eventChannel.DisposeAsync();
        }
    }

    public async Task RunDeadLetterViewerAsync(CancellationToken cancellationToken)
    {
        var factory = RabbitMqConnectionFactory.Create("message-broker-dead-letter-viewer");
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await RabbitMqTopology.DeclareBaseAsync(channel);
        await StartDeadLetterConsumerAsync(channel);

        Console.WriteLine($"[DLQ] Ascult coada {BrokerConstants.DeadLetterQueue}.");
        Console.WriteLine("Apasă Ctrl+C pentru oprire.");
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private static async Task StartQueueConsumerAsync(
        IChannel channel,
        string queueName,
        string? expectedRecipient,
        string displayName)
    {
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false);
        var acknowledgementLock = new SemaphoreSlim(1, 1);
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {

            var body = eventArgs.Body.ToArray();
            Exception? processingError = null;

            try
            {
                var xml = Encoding.UTF8.GetString(body);
                var message = MessageXmlSerializer.Deserialize(xml);
                MessageValidator.EnsureValid(message, expectedRecipient);

                Console.WriteLine();
                Console.WriteLine($"[RECEIVER] Mesaj primit prin {displayName}");
                Console.WriteLine($"ID: {message.MessageId}");
                Console.WriteLine($"Expeditor: {message.SenderId}");
                Console.WriteLine($"Destinatar: {message.RecipientId}");
                Console.WriteLine($"Conținut: {message.Payload}");
                Console.WriteLine("[MESSAGE TRANSLATOR] Reprezentare JSON:");
                Console.WriteLine(MessageTranslator.ToJson(message));
            }
            catch (Exception exception)
            {
                processingError = exception;
                Console.WriteLine($"[RECEIVER] Mesajul nu a putut fi procesat: {exception.Message}");
            }

            
            await acknowledgementLock.WaitAsync();
            try
            {
                if (processingError is null)
                {
                    await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
                    Console.WriteLine("[RECEIVER] ACK: mesaj procesat.");
                }
                else
                {
                    
                    await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false);
                    Console.WriteLine("[RECEIVER] NACK: mesajul a fost trimis în Dead Letter Queue.");
                }
            }
            catch (Exception acknowledgementException)
            {
                Console.WriteLine($"[RECEIVER] Eroare la confirmarea mesajului: {acknowledgementException.Message}");
            }
            finally
            {
                acknowledgementLock.Release();
            }
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer);
    }

    private static async Task StartDeadLetterConsumerAsync(IChannel channel)
    {
        await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false);
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var rawMessage = Encoding.UTF8.GetString(body);

            Console.WriteLine();
            Console.WriteLine("[DLQ] Mesaj nelivrat / respins:");
            Console.WriteLine(rawMessage);

            await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
        };

        await channel.BasicConsumeAsync(
            BrokerConstants.DeadLetterQueue,
            autoAck: false,
            consumer: consumer);
    }
}
