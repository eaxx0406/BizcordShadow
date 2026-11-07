namespace ChatService.Repositories
{
    // Simpel dataklasse, som Dapper fylder med én række fra tabellen participation.
    // Ingen regler – den bruges kun til at flytte data fra databasen over i Participation.Rehydrate.
    public class ParticipationRow
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid ParticipantId { get; set; }

        // DateTime og ikke DateTimeOffset: Npgsql læser timestamptz som DateTime (i UTC).
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
    }
}
