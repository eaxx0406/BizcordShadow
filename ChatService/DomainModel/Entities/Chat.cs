namespace ChatService.DomainModel.Entities
{
    public class Chat
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Relations
        public ICollection<Participation> Participations { get; set; }
            = new List<Participation>();

        public ICollection<Message> Messages { get; set; }
            = new List<Message>();
    }
}
