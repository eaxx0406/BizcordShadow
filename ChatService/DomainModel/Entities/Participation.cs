namespace ChatService.DomainModel.Entities
{
    // Del af Chat-aggregatet: en participants medlemskab af en chat i en periode.
    // Oprettes kun via Chat.AddParticipant (derfor internal konstruktor).
    public class Participation
    {
        public Guid Id { get; private set; }
        public Guid ChatId { get; private set; }
        public Guid ParticipantId { get; private set; }
        public DateTimeOffset JoinedAt { get; private set; }
        public DateTimeOffset? LeftAt { get; private set; }

        internal Participation(Guid chatId, Guid participantId)
        {
            Id = Guid.NewGuid();
            ChatId = chatId;
            ParticipantId = participantId;
            JoinedAt = DateTimeOffset.UtcNow;
        }

        // Tom konstruktor, som kun Rehydrate må bruge (derfor private).
        private Participation() { }

        // Rehydrering: genopbygger en EKSISTERENDE participation ud fra data fra databasen.
        // Beholder id og tidspunkter fra databasen i stedet for at lave nye. Bruges kun af repository'et.
        internal static Participation Rehydrate(Guid id, Guid chatId, Guid participantId, DateTimeOffset joinedAt, DateTimeOffset? leftAt)
        {
            Participation participation = new Participation();
            participation.Id = id;
            participation.ChatId = chatId;
            participation.ParticipantId = participantId;
            participation.JoinedAt = joinedAt;
            participation.LeftAt = leftAt;
            return participation;
        }
    }
}
