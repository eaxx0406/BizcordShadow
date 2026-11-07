namespace ChatService.DomainModel.Entities
{
    // Eget aggregate: refererer Chat og afsender med id'er, ikke objekter.
    public class Message
    {
        public Guid Id { get; private set; }
        public Guid ChatId { get; private set; }

        // Afsender er en Participation (medlemskabet), ikke direkte en Participant
        public Guid SenderId { get; private set; }

        public string Content { get; private set; } = string.Empty;
        public DateTimeOffset SentAt { get; private set; }

        public Message(Guid chatId, Guid senderId, string content)
        {
            Id = Guid.NewGuid();
            ChatId = chatId;
            SenderId = senderId;
            Content = content;
            SentAt = DateTimeOffset.UtcNow;
        }
    }
}
