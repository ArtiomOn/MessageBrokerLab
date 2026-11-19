using RabbitMQ.Client;

namespace MessageBroker.Common;

public static class RabbitMqTopology
{
    public static async Task DeclareBaseAsync(IChannel channel)
    {
        await channel.ExchangeDeclareAsync(
            BrokerConstants.DirectExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        await channel.ExchangeDeclareAsync(
            BrokerConstants.FanoutExchange,
            ExchangeType.Fanout,
            durable: true,
            autoDelete: false);

        await channel.ExchangeDeclareAsync(
            BrokerConstants.DeadLetterExchange,
            ExchangeType.Direct,
            durable: true,
            autoDelete: false);

        await channel.QueueDeclareAsync(
            BrokerConstants.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        await channel.QueueBindAsync(
            BrokerConstants.DeadLetterQueue,
            BrokerConstants.DeadLetterExchange,
            BrokerConstants.DeadLetterRoutingKey);
    }

    public static async Task<string> DeclareDirectReceiverAsync(IChannel channel, string receiverId)
    {
        MessageValidator.EnsureValidIdentifier(receiverId, nameof(receiverId));
        await DeclareBaseAsync(channel);

        var queueName = BrokerConstants.GetReceiverQueueName(receiverId);
        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = BrokerConstants.DeadLetterExchange,
            ["x-dead-letter-routing-key"] = BrokerConstants.DeadLetterRoutingKey
        };

        await channel.QueueDeclareAsync(
            queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments);

        await channel.QueueBindAsync(
            queueName,
            BrokerConstants.DirectExchange,
            BrokerConstants.GetReceiverRoutingKey(receiverId));

        return queueName;
    }

    public static async Task<string> DeclareSubscriberAsync(IChannel channel, string subscriberId)
    {
        MessageValidator.EnsureValidIdentifier(subscriberId, nameof(subscriberId));
        await DeclareBaseAsync(channel);

        var queueName = BrokerConstants.GetSubscriberQueueName(subscriberId);

        await channel.QueueDeclareAsync(
            queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = BrokerConstants.DeadLetterExchange,
                ["x-dead-letter-routing-key"] = BrokerConstants.DeadLetterRoutingKey
            });

        await channel.QueueBindAsync(
            queueName,
            BrokerConstants.FanoutExchange,
            routingKey: string.Empty);

        return queueName;
    }
}
