using RabbitMQ.Client;

namespace MessageBroker.Common;

public static class RabbitMqConnectionFactory
{
    public static ConnectionFactory Create(string clientName)
    {
        var portText = Environment.GetEnvironmentVariable("RABBITMQ_PORT");
        var port = int.TryParse(portText, out var parsedPort) ? parsedPort : 5672;

        return new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            Port = port,
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "labuser",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "labpass",
            VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/",
            ClientProvidedName = clientName,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            ConsumerDispatchConcurrency = 4
        };
    }
}
