using ChatService.DomainModel.Entities;
using ChatService.Repositories.Interfaces;
using Dapper;
using Npgsql;

namespace ChatService.Repositories
{
    public class MessageWriteRepository : IMessageWriteRepository
    {
        private readonly string _connectionString;

        public MessageWriteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChatDb")
                ?? throw new InvalidOperationException("Connection string 'ChatDb' mangler. Sæt den med dotnet user-secrets.");
        }

        public async Task AddAsync(Message message, CancellationToken cancellationToken)
        {
            //forbinde til databasen
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            //indsæt beskeden i databasen
            //(ingen transaction: det er kun én INSERT, og én SQL-sætning er altid alt-eller-intet)
            await connection.ExecuteAsync(new CommandDefinition(
                "INSERT INTO message (id, chat_id, sender_id, content, sent_at) VALUES (@Id, @ChatId, @SenderId, @Content, @SentAt)",
                message,
                cancellationToken: cancellationToken));
        }

        public async Task MarkDeletedAsync(Message message, CancellationToken cancellationToken)
        {
            //forbinde til databasen
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            //soft delete (ADR 0020): rækken bliver, men indholdet overskrives, så teksten er væk for altid
            //(@Content er tom og @DeletedAt er sat af Message.Delete – repository'et har ingen regler)
            await connection.ExecuteAsync(new CommandDefinition(
                "UPDATE message SET content = @Content, deleted_at = @DeletedAt WHERE id = @Id",
                message,
                cancellationToken: cancellationToken));
        }
    }
}
