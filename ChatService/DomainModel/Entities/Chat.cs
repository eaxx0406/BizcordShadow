namespace ChatService.DomainModel.Entities
{
    public class Chat
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;

        // Relations
        public ICollection<Participation> Participations { get; set; }
            = new List<Participation>();

        public ICollection<Message> Messages { get; set; }
            = new List<Message>();

        //konstruktor der laver en chat
        public Chat(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        public void AddParticipant(Guid participantId)
        {
            Participations.Add(new Participation
            {
                Id = Guid.NewGuid(),
                ChatId = Id,
                ParticipantId = participantId,
                JoinedAt = DateTimeOffset.UtcNow
            });
        }
    }
}
