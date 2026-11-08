using ChatService.Repositories.Interfaces;
using Dapper;
using Npgsql;

namespace ChatService.Repositories
{
    public class ParticipationReadRepository : IParticipationReadRepository
    {
        private readonly string _connectionString;

        public ParticipationReadRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChatDb")
                                ?? throw new InvalidOperationException("Connection string 'ChatDb' mangler. Sæt den med dotnet user-secrets.");
        }

        public async Task<Guid?> GetActiveIdAsync(Guid chatId, Guid userId, CancellationToken cancellationToken)
        {
            //Forbind til databasen
            await using var connection = new NpgsqlConnection(_connectionString);

            //Åbn forbindelsen
            await connection.OpenAsync(cancellationToken);

            //Find brugerens aktive participation i chatten (ux_participation_active sikrer, at der højst er én)
            //returnerer null, hvis brugeren ikke er aktivt medlem
            return await connection.QuerySingleOrDefaultAsync<Guid?>(new CommandDefinition(
                @"SELECT pa.id
                  FROM participation pa
                  JOIN participant p ON p.id = pa.participant_id
                  WHERE pa.chat_id = @ChatId AND p.user_id = @UserId AND pa.left_at IS NULL",
                new { ChatId = chatId, UserId = userId },
                cancellationToken: cancellationToken));
        }

        public async Task<List<Guid>> GetActiveUserIdsAsync(Guid chatId, CancellationToken cancellationToken)
        {
            //Forbind til databasen
            await using var connection = new NpgsqlConnection(_connectionString);

            //Åbn forbindelsen
            await connection.OpenAsync(cancellationToken);

            //Alle, der er med i chatten lige nu (left_at IS NULL = ikke forladt/smidt ud)
            return (await connection.QueryAsync<Guid>(new CommandDefinition(
                @"SELECT p.user_id
                  FROM participation pa
                  JOIN participant p ON p.id = pa.participant_id
                  WHERE pa.chat_id = @ChatId AND pa.left_at IS NULL",
                new { ChatId = chatId },
                cancellationToken: cancellationToken))).ToList();
        }
    }
}
