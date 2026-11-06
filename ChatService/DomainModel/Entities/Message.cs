namespace ChatService.DomainModel.Entities
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;

        public Guid ChatId { get; set; }
        public Chat Chat { get; set; } = null!;

        // Sender is a participation, not directly a participant
        public Guid SenderId { get; set; }
        public Participation Sender { get; set; } = null!;

        public DateTimeOffset SentAt { get; set; }

        // Delivery and read receipts
        public ICollection<MessageReceipt> Receipts { get; set; }
            = new List<MessageReceipt>();
    }
}
