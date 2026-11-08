using MessageClient.Extensions;
using RealTimeCommunicationService.Handlers;
using RealTimeCommunicationService.Messaging;
using RealTimeCommunicationService.Repositories;

// DUMMY: Vi leger, at en anden gruppe ejer denne service. Den skal bare bevise,
// at ChatService' MessageSent-events faktisk når frem gennem RabbitMQ.
var builder = WebApplication.CreateBuilder(args);

// Samme message client som ChatService. Connection string i user-secrets (se README).
var rabbitMqConnectionString = builder.Configuration.GetConnectionString("RabbitMQ")
    ?? throw new InvalidOperationException("Connection string 'RabbitMQ' mangler. Se README.");
builder.Services.AddMessageClient(rabbitMqConnectionString);

// Singleton: MessageListener lever hele appens levetid, så det gør dens handler og repository også.
// Det er OK, fordi repositoriet ikke holder på en forbindelse: det åbner en ny pr. kald.
builder.Services.AddSingleton<ReceivedMessageRepository>();
builder.Services.AddSingleton<MessageSentHandler>();
builder.Services.AddHostedService<MessageListener>();

// Dapper: map snake_case-kolonner (message_id) til PascalCase-properties (MessageId).
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

app.MapGet("/", () => "RealTimeCommunicationService (dummy) kører");

// Se loggen: hvilke MessageSent-events er kommet frem?
app.MapGet("/received", async (ReceivedMessageRepository repository) => await repository.GetAllAsync());

app.Run();
