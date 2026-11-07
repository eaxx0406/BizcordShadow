namespace ChatService.Repositories
{
    // Simpel dataklasse, som Dapper fylder med én række fra tabellen chat.
    // Ingen regler – den bruges kun til at flytte data fra databasen over i Chat.Rehydrate.
    public class ChatRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
