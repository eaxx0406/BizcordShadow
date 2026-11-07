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
    }
}
