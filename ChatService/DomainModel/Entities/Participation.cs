namespace ChatService.DomainModel.Entities
{
    public class Participation
    {
        public Guid Id { get; set; }

        public Guid ChatId { get; set; }
        public Chat Chat { get; set; } = null!;

        public Guid ParticipantId { get; set; }
        public Participant Participant { get; set; } = null!;

        public DateTimeOffset JoinedAt { get; set; }
        public DateTimeOffset? LeftAt { get; set; }

        // Messages sent during this participation
        public ICollection<Message> SentMessages { get; set; }
            = new List<Message>();
    }
}
