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
    //
    // RecipientUserIds: de aktive deltagere i chatten, da beskeden blev sendt (minus afsenderen).
    // ChatService ejer deltagelse (ADR 0010), så det er ChatService, der afgør, hvem der må få beskeden.
    // Modtagerne må IKKE selv regne det ud fra join/leave-events: kommer de i forkert rækkefølge,
    // kan en, der lige er smidt ud, stadig få beskeden (fx i et notifikations-preview).
    public record MessageSent(Guid MessageId, Guid ChatId, Guid SenderUserId, IReadOnlyList<Guid> RecipientUserIds, string Content, DateTimeOffset SentAt);
}
