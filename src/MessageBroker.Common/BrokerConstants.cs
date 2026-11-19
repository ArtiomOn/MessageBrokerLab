namespace MessageBroker.Common;

public static class BrokerConstants
{
    public const string DirectExchange = "lab.direct";
    public const string FanoutExchange = "lab.events";
    public const string DeadLetterExchange = "lab.dead-letter";

    public const string DeadLetterQueue = "lab.dead-letter.queue";
    public const string DeadLetterRoutingKey = "dead-letter";

    public const string DirectQueuePrefix = "receiver.";
    public const string SubscriberQueuePrefix = "subscriber.";

    public static string GetReceiverQueueName(string receiverId) =>
        $"{DirectQueuePrefix}{receiverId}";

    public static string GetReceiverRoutingKey(string receiverId) =>
        $"receiver.{receiverId}";

    public static string GetSubscriberQueueName(string subscriberId) =>
        $"{SubscriberQueuePrefix}{subscriberId}";
}
