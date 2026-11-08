namespace ChatService.Repositories
{
    // Simpel dataklasse, som Dapper fylder med én række fra message-tabellen.
    // Bruges til at genopbygge Message-aggregatet (Message.Rehydrate), når en besked skal ændres.
    public class MessageAggregateRow
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;

        // DateTime og ikke DateTimeOffset: Npgsql læser timestamptz som DateTime (i UTC).
        public DateTime SentAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
