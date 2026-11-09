namespace ChatService.Repositories
{
    // Simpel dataklasse, som Dapper fylder med én række fra besked-queryen.
    // Bruges kun til at flytte data fra databasen over i ChatMessageDto.
    public class MessageRow
    {
        public Guid Id { get; set; }
        public Guid SenderUserId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        // DateTime og ikke DateTimeOffset: Npgsql læser timestamptz som DateTime (i UTC).
        public DateTime SentAt { get; set; }

        // Sat, hvis beskeden er slettet (ADR 0020)
        public DateTime? DeletedAt { get; set; }

        // Sat, hvis beskeden er rettet
        public DateTime? EditedAt { get; set; }

        // Systembeskeder (ADR 0022). Details er jsonb læst som tekst (se DetailsJson)
        public string Kind { get; set; } = string.Empty;
        public string? SystemType { get; set; }
        public string? Details { get; set; }
    }
}
