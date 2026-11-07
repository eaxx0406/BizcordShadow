namespace ChatService.DomainModel.Entities
{
    // Leverings- og læsekvittering for én modtager af én besked.
    // Ingen egen Id: identiteten er (MessageId, ReceiverId), samme som primary key i databasen.
    public class MessageReceipt
    {
        public Guid MessageId { get; private set; }

        // Modtager er en Participant, uafhængig af medlemskabshistorik
        public Guid ReceiverId { get; private set; }

        public DateTimeOffset? DeliveredAt { get; private set; }
        public DateTimeOffset? SeenAt { get; private set; }

        public MessageReceipt(Guid messageId, Guid receiverId)
        {
            MessageId = messageId;
            ReceiverId = receiverId;
        }
    }
}
