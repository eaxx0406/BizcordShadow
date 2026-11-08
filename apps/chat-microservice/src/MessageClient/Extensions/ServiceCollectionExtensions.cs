using EasyNetQ;
using MessageClient.Implementation;
using MessageClient.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MessageClient.Extensions
{
    public static class ServiceCollectionExtensions
    {
        // Gør det muligt at skrive builder.Services.AddMessageClient(connectionString) i Program.cs
        // ("this IServiceCollection" gør det til en extension method, ligesom AddControllers/AddMediatR).
        //
        // Hvorfor en extension og ikke bare registreringerne direkte i Program.cs?
        // - Indkapsling: RabbitMqMessageClient er internal, så servicen KAN ikke registrere den selv.
        //   Servicen kender kun AddMessageClient og IMessageClient, ikke at der er RabbitMQ/EasyNetQ bagved.
        // - Biblioteket kender sine egne regler (IBus, JSON, Singleton), så ingen service kan glemme dem.
        // - Genbrug: andre services (fx NotificationService) bruger samme ene linje.
        // - Udskiftning: skifter vi til fx Kafka, ændres kun denne metode, ingen Program.cs.
        public static IServiceCollection AddMessageClient(this IServiceCollection services, string connectionString)
        {
            // 1. Forbindelsen til posthuset: registrerer EasyNetQ's IBus.
            //    Beskederne sendes som JSON (System.Text.Json).
            services.AddEasyNetQ(connectionString).UseSystemTextJson();

            // 2. Sekretæren: når nogen beder om IMessageClient, får de vores RabbitMQ-version.
            //    Singleton: én for hele appen, ligesom IBus (forbindelsen skal genbruges, ikke åbnes pr. request).
            services.AddSingleton<IMessageClient, RabbitMqMessageClient>();

            return services;
        }
    }
}
