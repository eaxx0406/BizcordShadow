using ChatService.DomainModel.ValueObjects;

namespace ChatService.DomainModel.Entities
{
    // Del af Chat-aggregatet: en participants medlemskab af en chat i en periode.
    // Oprettes kun via Chat.AddParticipant (derfor internal konstruktor).
    public class Participation
    {
        public Guid Id { get; private set; }
        public Guid ChatId { get; private set; }
        public Guid ParticipantId { get; private set; }

        // Hvornår medlemskabet startede og evt. sluttede – et value object, så reglerne ligger ét sted
        public MembershipPeriod Period { get; private set; } = null!;

        // Genveje til perioden, så Dapper stadig kan finde @JoinedAt og @LeftAt i SQL'en
        public DateTimeOffset JoinedAt => Period.JoinedAt;
        public DateTimeOffset? LeftAt => Period.LeftAt;

        internal Participation(Guid chatId, Guid participantId)
        {
            Id = Guid.NewGuid();
            ChatId = chatId;
            ParticipantId = participantId;
            Period = new MembershipPeriod(DateTimeOffset.UtcNow);
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
            participation.Period = new MembershipPeriod(joinedAt, leftAt);
            return participation;
        }

        // Afslutter medlemskabet. Internal: kun Chat (aggregate root) må kalde den, via Chat.Leave.
        internal void Leave()
        {
            // Perioden udskiftes med en ny, afsluttet periode – value objects ændres ikke
            Period = Period.End(DateTimeOffset.UtcNow);
        }
    }
}
