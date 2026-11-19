using MessageBroker.Receiver;

Console.OutputEncoding = System.Text.Encoding.UTF8;

if (args.Length == 0 || args[0] is "--help" or "-h")
{
    PrintUsage();
    return;
}

using var cancellationSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationSource.Cancel();
};

var consumer = new MessageConsumer();

try
{
    switch (args[0].ToLowerInvariant())
    {
        case "receiver":
        {
            if (args.Length < 2)
            {
                PrintUsage();
                Environment.ExitCode = 1;
                return;
            }

            var receiverId = args[1];
            var subscribe = args.Skip(2).Any(argument =>
                string.Equals(argument, "--subscribe", StringComparison.OrdinalIgnoreCase));

            await consumer.RunReceiverAsync(receiverId, subscribe, cancellationSource.Token);
            break;
        }
        case "dlq":
            await consumer.RunDeadLetterViewerAsync(cancellationSource.Token);
            break;
        default:
            PrintUsage();
            Environment.ExitCode = 1;
            break;
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("[RECEIVER] Aplicația a fost oprită.");
}
catch (Exception exception)
{
    Console.Error.WriteLine($"[EROARE] {exception.Message}");
    Console.Error.WriteLine("Verifică dacă RabbitMQ rulează și dacă setările de conexiune sunt corecte.");
    Environment.ExitCode = 1;
}

static void PrintUsage()
{
    Console.WriteLine("MessageBroker.Receiver");
    Console.WriteLine();
    Console.WriteLine("Comenzi:");
    Console.WriteLine("  dotnet run --project src/MessageBroker.Receiver -- receiver user1");
    Console.WriteLine("  dotnet run --project src/MessageBroker.Receiver -- receiver user1 --subscribe");
    Console.WriteLine("  dotnet run --project src/MessageBroker.Receiver -- dlq");
}
