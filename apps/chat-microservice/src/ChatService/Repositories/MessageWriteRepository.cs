using ChatService.DomainModel.Entities;
using ChatService.Repositories.Interfaces;
using Dapper;
using Npgsql;
using System.Text.Json;

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

            //indsæt beskeden i databasen (både almindelige beskeder og systembeskeder, ADR 0022)
            //(ingen transaction: det er kun én INSERT, og én SQL-sætning er altid alt-eller-intet)
            //Details er en ordbog i C# og jsonb i databasen: vi sender den som JSON-tekst og caster den i SQL'en
            await connection.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO message (id, chat_id, sender_id, content, sent_at, kind, system_type, details)
                  VALUES (@Id, @ChatId, @SenderId, @Content, @SentAt, @Kind, @SystemType, CAST(@Details AS jsonb))",
                new
                {
                    message.Id,
                    message.ChatId,
                    message.SenderId,
                    message.Content,
                    message.SentAt,
                    message.Kind,
                    message.SystemType,
                    Details = message.Details is null ? null : JsonSerializer.Serialize(message.Details)
                },
                cancellationToken: cancellationToken));
        }

        public async Task MarkDeletedAsync(Message message, CancellationToken cancellationToken)
        {
            //forbinde til databasen
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            //normal sletning (ADR 0021): rækken bliver, men teksten overskrives, så den er væk for altid
            //(@Content er tom og @DeletedAt er sat af Message.Delete – repository'et har ingen regler)
            await connection.ExecuteAsync(new CommandDefinition(
                "UPDATE message SET content = @Content, deleted_at = @DeletedAt WHERE id = @Id",
                message,
                cancellationToken: cancellationToken));
        }
    }
}
