namespace ChatService.DomainModel.Entities
{
    public class MessageReceipt
    {
        public Guid Id { get; set; }

        public Guid MessageId { get; set; }
        public Message Message { get; set; } = null!;

        // Receiver is a participant, independent of participation history
        public Guid ReceiverId { get; set; }
        public Participant Receiver { get; set; } = null!;

        public DateTimeOffset? DeliveredAt { get; set; }
        public DateTimeOffset? SeenAt { get; set; }
    }
}
