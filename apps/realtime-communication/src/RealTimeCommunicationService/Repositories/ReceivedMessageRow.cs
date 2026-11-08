namespace RealTimeCommunicationService.Repositories
{
    // Én række i received_message, som Dapper læser den.
    public class ReceivedMessageRow
    {
        public Guid MessageId { get; set; }
        public Guid ChatId { get; set; }
        public Guid SenderUserId { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
