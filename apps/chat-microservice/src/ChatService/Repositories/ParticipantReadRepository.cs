using ChatService.Repositories.Interfaces;
using Dapper;
using Npgsql;

namespace ChatService.Repositories
{
    public class ParticipantReadRepository : IParticipantReadRepository
    {
        private readonly string _connectionString;

        public ParticipantReadRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChatDb")
                                ?? throw new InvalidOperationException("Connection string 'ChatDb' mangler. Sæt den med dotnet user-secrets.");
        }

        public async Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            //Forbind til databasen
            await using var connection = new NpgsqlConnection(_connectionString);

            //Åbn forbindelsen
            await connection.OpenAsync(cancellationToken);

            //Find participant-id'et for brugeren (user_id er unik, så der er højst én)
            //returnerer null, hvis brugeren ikke findes
            return await connection.QuerySingleOrDefaultAsync<Guid?>(new CommandDefinition(
                "SELECT id FROM participant WHERE user_id = @UserId",
                new { UserId = userId },
                cancellationToken: cancellationToken));
        }
    }
}
