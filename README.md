# Message Broker Lab 2 (C# + RabbitMQ)

Proiect demonstrativ pentru laboratorul „Agent de mesagerie - Message Broker”. RabbitMQ asigură rolul de broker, iar aplicațiile Sender și Receiver sunt scrise în C#/.NET.

## 1. Cerințe

- .NET 10 SDK
- Docker Desktop pentru macOS
- Visual Studio Code (opțional, pentru editarea codului)

Biblioteca folosită: `RabbitMQ.Client` 7.2.2.

## 2. Pornirea RabbitMQ

Din directorul rădăcină al proiectului:

```bash
docker compose up -d
```

Interfața RabbitMQ Management:

- URL: http://localhost:15672
- Username: `labuser`
- Password: `labpass`

Aplicațiile C# se conectează implicit la `localhost:5672` cu aceleași credențiale.

Pentru a opri RabbitMQ:

```bash
docker compose down
```

Pentru a șterge și datele persistente ale brokerului (cozi și mesaje):

```bash
docker compose down -v
```

## 3. Restaurarea și compilarea proiectului

```bash
dotnet restore MessageBrokerLab.sln
dotnet build MessageBrokerLab.sln
```

## 4. Testarea comunicării unu-la-unu (Direct Exchange)

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

## 5. Testarea Publish-Subscribe (Fanout Exchange)

Pornește două terminale și rulează două instanțe Receiver:

```bash
dotnet run --project src/MessageBroker.Receiver -- receiver user1 --subscribe
```

```bash
dotnet run --project src/MessageBroker.Receiver -- receiver user2 --subscribe
```

Pornește Sender și alege opțiunea `2`. Evenimentul ar trebui să apară în ambele instanțe Receiver.

Important: abonații trebuie să fie porniți înainte de publicarea evenimentului. Fiecare abonat are propria coadă durabilă.

## 6. Testarea Dead Letter Queue

Pornește Receiver:

```bash
dotnet run --project src/MessageBroker.Receiver -- receiver user1
```

În alt terminal, pornește vizualizatorul cozii de mesaje respinse:

```bash
dotnet run --project src/MessageBroker.Receiver -- dlq
```

În Sender, alege opțiunea `3` și introdu `user1`. Sender publică intenționat XML malformat. Receiver nu îl poate deserializa și trimite NACK cu `requeue: false`. RabbitMQ mută mesajul în Dead Letter Queue.

## 7. Setări RabbitMQ prin variabile de mediu

Valorile implicite sunt potrivite pentru `docker-compose.yml`. Le poți schimba astfel:

```bash
export RABBITMQ_HOST=localhost
export RABBITMQ_PORT=5672
export RABBITMQ_USER=labuser
export RABBITMQ_PASSWORD=labpass
export RABBITMQ_VHOST=/
```

## 8. Ce demonstrează implementarea

| Cerință / șablon | Unde este demonstrat |
|---|---|
| Format XML | `BrokerMessage` și `MessageXmlSerializer` |
| Validarea mesajelor | `MessageValidator` |
| Comunicare asincronă | API-ul `RabbitMQ.Client` 7.x, `async` / `await` |
| Comunicare unu-la-unu | Direct Exchange și cozile `receiver.*` |
| Comunicare unu-la-mulți | Fanout Exchange și cozile `subscriber.*` |
| Stocare durabilă | Durable queues și `DeliveryMode = 2` |
| Dead Letter Channel | `lab.dead-letter` și `lab.dead-letter.queue` |
| Message Translator | XML este deserializat într-un obiect și reprezentat apoi ca JSON |
| Tratare concurentă | Consumatori asincroni; mai multe instanțe Receiver pot procesa în paralel |

## 9. Observații importante

- RabbitMQ este brokerul. Nu implementăm manual socket-uri TCP sau o coadă proprie, deoarece profesorul a cerut utilizarea RabbitMQ.
- Cozile și exchange-urile sunt declarate de aplicații dacă nu există deja.
- Pentru mesajele invalide, Receiver folosește `BasicNack(..., requeue: false)`, iar configurația Dead Letter Exchange trimite mesajul în coada DLQ.
- În această versiune mesajele respinse ajung direct în DLQ; nu este implementată o coadă intermediară de retry cu TTL.
- Pentru o demonstrație mai clară a persistenței, oprește și pornește din nou containerul RabbitMQ fără `docker compose down -v`. Datele volumului se păstrează.
- Aceasta este o bază funcțională pentru laborator. Cerința exactă privind varianta 9 trebuie verificată separat, deoarece documentul PDF furnizat nu conține tabelul variantelor.
