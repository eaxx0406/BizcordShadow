namespace ChatService.DomainModel.Entities
{
    // Read model af en User fra UserService (source of truth) – se ADR 0003.
    // 1:1 med User. Opdateres fra events fra UserService, ikke af ChatService selv.
    public class Participant
    {
        public Guid Id { get; private set; }

        // Reference til User i UserService
        public Guid UserId { get; private set; }
        public string Name { get; private set; } = string.Empty;

        public Participant(Guid userId, string name)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Name = name;
        }
    }
}
