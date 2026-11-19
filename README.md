# Message Broker Lab 2 (C# + RabbitMQ)

## 1. Cerințe

- .NET 10 SDK
- Docker Desktop

Biblioteca folosită: `RabbitMQ.Client` 7.2.2.

## 2. Pornirea RabbitMQ

Din directorul rădăcină al proiectului:

```bash
docker compose up -d
```

## 3. Testarea comunicării unu-la-unu (Direct Exchange)

Deschide primul terminal și pornește receptorul `user1`:

```bash
dotnet run --project src/MessageBroker.Receiver -- receiver user1
```

Deschide al doilea terminal și pornește Sender:

```bash
dotnet run --project src/MessageBroker.Sender
```

Alege opțiunea `1`, introdu `user1` ca destinatar și scrie mesajul. Mesajul trebuie să apară în terminalul Receiver.

Poți porni mai multe instanțe Receiver cu identificatori diferiți, de exemplu `user1` și `user2`. Fiecare primește numai mesajele rutate către coada sa.

## 4. Testarea Publish-Subscribe (Fanout Exchange)

Pornește două terminale și rulează două instanțe Receiver:

```bash
dotnet run --project src/MessageBroker.Receiver -- receiver user1 --subscribe
```

```bash
dotnet run --project src/MessageBroker.Receiver -- receiver user2 --subscribe
```

Pornește Sender și alege opțiunea `2`. Evenimentul ar trebui să apară în ambele instanțe Receiver.

Important: abonații trebuie să fie porniți înainte de publicarea evenimentului. Fiecare abonat are propria coadă durabilă.

## 5. Testarea Dead Letter Queue

Pornește Receiver:

```bash
dotnet run --project src/MessageBroker.Receiver -- receiver user1
```

În alt terminal, pornește vizualizatorul cozii de mesaje respinse:

```bash
dotnet run --project src/MessageBroker.Receiver -- dlq
```

În Sender, alege opțiunea `3` și introdu `user1`. Sender publică intenționat XML malformat. Receiver nu îl poate deserializa și trimite NACK cu `requeue: false`. RabbitMQ mută mesajul în Dead Letter Queue.

## 6. Setări RabbitMQ prin variabile de mediu

Valorile implicite sunt potrivite pentru `docker-compose.yml`. Le poți schimba astfel:

```bash
export RABBITMQ_HOST=localhost
export RABBITMQ_PORT=5672
export RABBITMQ_USER=labuser
export RABBITMQ_PASSWORD=labpass
export RABBITMQ_VHOST=/
```