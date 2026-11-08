using ChatService.DomainModel.Exceptions;

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

        // Soft delete (ADR 0020): beskeden bliver i databasen, men indholdet fjernes.
        // null = ikke slettet
        public DateTimeOffset? DeletedAt { get; private set; }
        public bool IsDeleted => DeletedAt is not null;

        public Message(Guid chatId, Guid senderId, string content)
        {
            Id = Guid.NewGuid();
            ChatId = chatId;
            SenderId = senderId;
            Content = content;
            SentAt = DateTimeOffset.UtcNow;
        }

        // Tom konstruktor, som kun Rehydrate må bruge (derfor private).
        private Message() { }

        // Rehydrering: genopbygger en EKSISTERENDE besked ud fra data fra databasen. Bruges kun af repository'et.
        public static Message Rehydrate(Guid id, Guid chatId, Guid senderId, string content, DateTimeOffset sentAt, DateTimeOffset? deletedAt)
        {
            Message message = new Message();
            message.Id = id;
            message.ChatId = chatId;
            message.SenderId = senderId;
            message.Content = content;
            message.SentAt = sentAt;
            message.DeletedAt = deletedAt;
            return message;
        }

        // Afsenderen sletter sin besked. byParticipationId er det AKTIVE medlemskab hos den, der vil slette.
        public void Delete(Guid byParticipationId)
        {
            // Regel: kun afsenderen må slette sin egen besked
            if (byParticipationId != SenderId)
            {
                throw new ForbiddenException("Du kan kun slette dine egne beskeder.");
            }

            // Allerede slettet: intet at gøre. DELETE må gerne kaldes flere gange med samme resultat (idempotent)
            if (IsDeleted)
            {
                return;
            }

            // Indholdet fjernes for altid. Rækken bliver, så paging (before=<id>) og historikken stadig hænger sammen
            Content = string.Empty;
            DeletedAt = DateTimeOffset.UtcNow;
        }
    }
}
