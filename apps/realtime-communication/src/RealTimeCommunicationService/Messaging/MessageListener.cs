using ChatService.Contracts.Events;
using MessageClient.Interfaces;
using RealTimeCommunicationService.Handlers;

namespace RealTimeCommunicationService.Messaging
{
    // Sætter postkassen op, når servicen starter.
    // BackgroundService: ASP.NET starter den automatisk ved opstart og lader den køre ved siden af web-delen.
    public class MessageListener : BackgroundService
    {
        // Navnet vi skriver på vores postkasse. EasyNetQ laver queuen ud fra event-typen + dette navn.
        // Samme navn = samme queue, så beskeder venter på os, mens servicen er nede.
        private const string SubscriptionId = "realtime-communication";

        private readonly IMessageClient _messageClient;
        private readonly MessageSentHandler _handler;

        public MessageListener(IMessageClient messageClient, MessageSentHandler handler)
        {
            _messageClient = messageClient;
            _handler = handler;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // "Når der kommer et MessageSent-brev, så giv det til handleren."
            // Vi kalder ikke handleren selv: EasyNetQ gør det, hver gang et brev lander i queuen.
            await _messageClient.Subscribe<MessageSent>(SubscriptionId, _handler.HandleAsync);
        }
    }
}
