using ChatService.Contracts;
using ChatService.DomainModel.Exceptions;

namespace ChatService.DomainModel.Entities
{
    // Eget aggregate: refererer Chat og afsender med id'er, ikke objekter.
    public class Message
    {
        public Guid Id { get; private set; }
        public Guid ChatId { get; private set; }

        // Afsender er en Participation (medlemskabet), ikke direkte en Participant.
        // For systembeskeder er det den, det handler om (den tilføjede, den der forlod, den der omdøbte).
        public Guid SenderId { get; private set; }

        // Afsenderens user-id. Gemmes ikke i message-tabellen (den kendes via SenderId -> participation -> participant),
        // men Message skal kende den for at kunne afgøre, hvem der må slette beskeden.
        public Guid SenderUserId { get; private set; }

        public string Content { get; private set; } = string.Empty;
        public DateTimeOffset SentAt { get; private set; }

        // Normal sletning (ADR 0020, 0021): rækken bliver, men teksten fjernes for altid. null = ikke slettet
        public DateTimeOffset? DeletedAt { get; private set; }
        public bool IsDeleted => DeletedAt is not null;

        // Sat, når afsenderen har rettet beskeden. null = aldrig rettet
        public DateTimeOffset? EditedAt { get; private set; }

        // Så længe efter afsendelse må man rette sin besked (ADR 0020)
        public static readonly TimeSpan EditWindow = TimeSpan.FromMinutes(15);

        // Systembeskeder (ADR 0022): Kind er "user" eller "system" (MessageKinds).
        // SystemType siger, hvad der skete (SystemMessageTypes), og Details har data om det (fx det nye navn).
        // Vi gemmer data og ikke færdig tekst – klienten laver teksten.
        public string Kind { get; private set; } = MessageKinds.User;
        public string? SystemType { get; private set; }
        public IReadOnlyDictionary<string, string>? Details { get; private set; }
        public bool IsSystem => Kind == MessageKinds.System;

        // En almindelig besked, som en person skriver
        public Message(Guid chatId, Guid senderId, Guid senderUserId, string content)
        {
            Id = Guid.NewGuid();
            ChatId = chatId;
            SenderId = senderId;
            SenderUserId = senderUserId;
            Content = content;
            SentAt = DateTimeOffset.UtcNow;
        }

        // Tom konstruktor, som kun Rehydrate og systembeskederne må bruge (derfor private).
        private Message() { }

        // --- Systembeskeder: én metode per hændelse, så det er tydeligt, hvilke der findes ---

        // "Oliver blev tilføjet". senderId = den nye participation
        public static Message MemberAdded(Guid chatId, Guid senderId, Guid senderUserId)
            => CreateSystem(chatId, senderId, senderUserId, SystemMessageTypes.MemberAdded, null);

        // "Kasper forlod chatten". senderId = den participation, der blev afsluttet
        public static Message MemberLeft(Guid chatId, Guid senderId, Guid senderUserId)
            => CreateSystem(chatId, senderId, senderUserId, SystemMessageTypes.MemberLeft, null);

        // "Pia omdøbte chatten til ...". senderId = Pias participation
        public static Message ChatRenamed(Guid chatId, Guid senderId, Guid senderUserId, string oldName, string newName)
            => CreateSystem(chatId, senderId, senderUserId, SystemMessageTypes.ChatRenamed,
                new Dictionary<string, string> { ["oldName"] = oldName, ["newName"] = newName });

        private static Message CreateSystem(Guid chatId, Guid senderId, Guid senderUserId, string systemType, IReadOnlyDictionary<string, string>? details)
        {
            Message message = new Message();
            message.Id = Guid.NewGuid();
            message.ChatId = chatId;
            message.SenderId = senderId;
            message.SenderUserId = senderUserId;
            message.SentAt = DateTimeOffset.UtcNow;
            message.Kind = MessageKinds.System;
            message.SystemType = systemType;
            message.Details = details;
            return message;
        }

        // Rehydrering: genopbygger en EKSISTERENDE besked ud fra data fra databasen. Bruges kun af repository'et.
        public static Message Rehydrate(Guid id, Guid chatId, Guid senderId, Guid senderUserId, string content, DateTimeOffset sentAt,
            DateTimeOffset? deletedAt, DateTimeOffset? editedAt, string kind, string? systemType, IReadOnlyDictionary<string, string>? details)
        {
            Message message = new Message();
            message.Id = id;
            message.ChatId = chatId;
            message.SenderId = senderId;
            message.SenderUserId = senderUserId;
            message.Content = content;
            message.SentAt = sentAt;
            message.DeletedAt = deletedAt;
            message.EditedAt = editedAt;
            message.Kind = kind;
            message.SystemType = systemType;
            message.Details = details;
            return message;
        }

        // Afsenderen retter sin besked. Den gamle tekst gemmes ikke (samme tankegang som ADR 0021).
        public void Edit(Guid byUserId, string newContent)
        {
            // Regel: kun afsenderens egne, almindelige beskeder, som ikke er slettet.
            // Samme tekst i alle tilfælde: vi fortæller ikke hvorfor (ADR 0013)
            if (IsSystem || IsDeleted || byUserId != SenderUserId)
            {
                throw new ForbiddenException("Beskeden kunne ikke redigeres.");
            }

            // Regel: kun inden for EditWindow efter afsendelse. Det er ens egen besked, så her må vi gerne sige hvorfor (409)
            if (DateTimeOffset.UtcNow > SentAt + EditWindow)
            {
                throw new ConflictException($"En besked kan kun redigeres i {EditWindow.TotalMinutes} minutter, efter den er sendt.");
            }

            Content = newContent;
            EditedAt = DateTimeOffset.UtcNow;
        }

        // Sletter beskeden. Alle regler for, hvem der må slette, samles her (fx en moderator senere).
        public void Delete(Guid byUserId)
        {
            // Regel: kun afsenderen må slette sin besked – uanset om man stadig er med i chatten.
            // Regel: systembeskeder kan ikke slettes – de er historik om chatten, ikke noget nogen har skrevet.
            // Samme tekst som når beskeden ikke findes: vi fortæller ikke hvorfor (ADR 0013)
            if (IsSystem || byUserId != SenderUserId)
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
