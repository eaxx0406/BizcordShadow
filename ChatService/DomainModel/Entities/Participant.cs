namespace ChatService.DomainModel.Entities
{
    public class Participant
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Reference to User in another service
        public Guid UserId { get; set; }

        // Relations
        public ICollection<Participation> Participations { get; set; }
            = new List<Participation>();

        public ICollection<MessageReceipt> MessageReceipts { get; set; }
            = new List<MessageReceipt>();
    }
}
