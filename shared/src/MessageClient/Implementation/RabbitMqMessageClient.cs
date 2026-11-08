using EasyNetQ;
using MessageClient.Interfaces;

namespace MessageClient.Implementation
{
    // "Sekretæren" der taler med posthuset (RabbitMQ) via EasyNetQ.
    // internal: resten af systemet kender kun IMessageClient, ikke at det er RabbitMQ bagved.
    internal class RabbitMqMessageClient : IMessageClient
    {
        private readonly IBus _bus;

        public RabbitMqMessageClient(IBus bus)
        {
            _bus = bus;
        }

        public async Task Publish<T>(T domainEventMessage)
        {
            // EasyNetQ bruger selv typen T til at finde exchangen.
            // En ny event-type kræver derfor ingen ændringer her.
            await _bus.PubSub.PublishAsync(domainEventMessage);
        }

        public async Task Subscribe<T>(string subscriptionId, Func<T, Task> handler)
        {
            // EasyNetQ opretter/finder postkassen (queue) ud fra T + subscriptionId
            // og kalder vores handler, hver gang der ligger et brev.
            await _bus.PubSub.SubscribeAsync(subscriptionId, handler);
        }
    }
}
