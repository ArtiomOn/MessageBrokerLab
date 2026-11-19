using System.Text;
using MessageBroker.Common;
using RabbitMQ.Client;

namespace MessageBroker.Sender;

public sealed class MessagePublisher : IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly SemaphoreSlim _publishLock = new(1, 1);

    private MessagePublisher(IConnection connection, IChannel channel)
    {
        _connection = connection;
        _channel = channel;

        _channel.BasicReturnAsync += (_, ea) =>
        {
            Console.WriteLine(
                $"[RABBITMQ] Mesaj nerutat: {ea.ReplyText}. " +
                $"Routing key: {ea.RoutingKey}"
            );

            return Task.CompletedTask;
        };
    }

    public static async Task<MessagePublisher> CreateAsync()
    {
        var factory = RabbitMqConnectionFactory.Create("message-broker-sender");
        var connection = await factory.CreateConnectionAsync();

        try
        {
            var channel = await connection.CreateChannelAsync();
            await RabbitMqTopology.DeclareBaseAsync(channel);
            return new MessagePublisher(connection, channel);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public async Task PublishDirectAsync(BrokerMessage message)
    {
        MessageValidator.EnsureValid(message);
        var routingKey = BrokerConstants.GetReceiverRoutingKey(message.RecipientId);
        await PublishXmlAsync(
            BrokerConstants.DirectExchange,
            routingKey,
            MessageXmlSerializer.Serialize(message));

        Console.WriteLine($"[SENDER] Mesajul {message.MessageId} a fost publicat pentru '{message.RecipientId}'.");
    }

    public async Task PublishEventAsync(BrokerMessage message)
    {
        MessageValidator.EnsureValid(message);
        await PublishXmlAsync(
            BrokerConstants.FanoutExchange,
            routingKey: string.Empty,
            MessageXmlSerializer.Serialize(message));

        Console.WriteLine($"[SENDER] Evenimentul {message.MessageId} a fost publicat către abonați.");
    }

    public async Task PublishInvalidXmlAsync(string receiverId)
    {
        MessageValidator.EnsureValidIdentifier(receiverId, nameof(receiverId));

       const string invalidXml = "<message><messageId>broken-message</message>";
        await PublishXmlAsync(
            BrokerConstants.DirectExchange,
            BrokerConstants.GetReceiverRoutingKey(receiverId),
            invalidXml);

        Console.WriteLine("[SENDER] XML-ul invalid a fost publicat pentru testarea Dead Letter Queue.");
    }

    private async Task PublishXmlAsync(string exchange, string routingKey, string xml)
    {
        var body = Encoding.UTF8.GetBytes(xml);
        var properties = new BasicProperties
        {
            ContentType = "application/xml",
            ContentEncoding = "utf-8",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = Guid.NewGuid().ToString("N")
        };

        await _publishLock.WaitAsync();
        try
        {
            await _channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _publishLock.Dispose();
        await _channel.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
