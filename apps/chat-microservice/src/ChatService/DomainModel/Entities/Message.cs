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

        // Afsenderens user-id. Gemmes ikke i message-tabellen (den kendes via SenderId -> participation -> participant),
        // men Message skal kende den for at kunne afgøre, hvem der må slette beskeden.
        public Guid SenderUserId { get; private set; }

        public string Content { get; private set; } = string.Empty;
        public DateTimeOffset SentAt { get; private set; }

        // Normal sletning (ADR 0020, 0021): rækken bliver, men teksten fjernes for altid. null = ikke slettet
        public DateTimeOffset? DeletedAt { get; private set; }
        public bool IsDeleted => DeletedAt is not null;

        public Message(Guid chatId, Guid senderId, Guid senderUserId, string content)
        {
            Id = Guid.NewGuid();
            ChatId = chatId;
            SenderId = senderId;
            SenderUserId = senderUserId;
            Content = content;
            SentAt = DateTimeOffset.UtcNow;
        }

        // Tom konstruktor, som kun Rehydrate må bruge (derfor private).
        private Message() { }

        // Rehydrering: genopbygger en EKSISTERENDE besked ud fra data fra databasen. Bruges kun af repository'et.
        public static Message Rehydrate(Guid id, Guid chatId, Guid senderId, Guid senderUserId, string content, DateTimeOffset sentAt, DateTimeOffset? deletedAt)
        {
            Message message = new Message();
            message.Id = id;
            message.ChatId = chatId;
            message.SenderId = senderId;
            message.SenderUserId = senderUserId;
            message.Content = content;
            message.SentAt = sentAt;
            message.DeletedAt = deletedAt;
            return message;
        }

        // Sletter beskeden. Alle regler for, hvem der må slette, samles her (fx en moderator senere).
        public void Delete(Guid byUserId)
        {
            // Regel: kun afsenderen må slette sin besked – uanset om man stadig er med i chatten.
            // Samme tekst som når beskeden ikke findes: vi fortæller ikke hvorfor (ADR 0013)
            if (byUserId != SenderUserId)
            {
                throw new ForbiddenException("Beskeden kunne ikke slettes.");
            }

            // Allerede slettet: intet at gøre (idempotent), og det første tidspunkt bevares
            if (IsDeleted)
            {
                return;
            }

            Content = string.Empty;
            DeletedAt = DateTimeOffset.UtcNow;
        }
    }
}
