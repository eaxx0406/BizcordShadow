namespace ChatService.Contracts.Events
{
    // Integration event: "brevet" ChatService publisher til RabbitMQ, når en besked er gemt.
    // Ligger i Contracts, fordi andre services (fx en notification- eller realtime-service) skal kende typen
    // for at kunne subscribe på den. EasyNetQ bruger typens navn til at finde exchangen.
    //
    // Navngivet i datid (MessageSent, ikke SendMessage): et event fortæller, at noget ER sket,
    // en command beder om, at noget skal ske.
    //
    // SenderUserId og ikke ParticipationId: andre services kender brugere, ikke ChatService' interne medlemskaber.
    public record MessageSent(Guid MessageId, Guid ChatId, Guid SenderUserId, string Content, DateTimeOffset SentAt);
}
