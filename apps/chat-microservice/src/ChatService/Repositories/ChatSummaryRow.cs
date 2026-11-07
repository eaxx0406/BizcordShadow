namespace ChatService.Repositories
{
    // Simpel dataklasse, som Dapper fylder med én række fra "mine chats"-queryen.
    // Bruges kun til at flytte data fra databasen over i ChatSummaryDto.
    public class ChatSummaryRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ParticipantCount { get; set; }
        public string? LastMessage { get; set; }
        public string? LastMessageSenderName { get; set; }

        // DateTime og ikke DateTimeOffset: Npgsql læser timestamptz som DateTime (i UTC).
        public DateTime? LastMessageAt { get; set; }
    }
}
