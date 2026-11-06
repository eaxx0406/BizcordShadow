using ChatService.DomainModel.Entities;
using ChatService.Repositories.Interfaces;
using Dapper;
using Npgsql;

namespace ChatService.Repositories
{
    public class ChatWriteRepository: IChatWriteRepository
    {
        private readonly string _connectionString;

        public ChatWriteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChatDb")
                ?? throw new InvalidOperationException("Connection string 'ChatDb' mangler. Sæt den med dotnet user-secrets.");
        }

        public async Task CreateAsync(Chat entity)
        {
            //forbinde til databasen
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            //start en new transaction
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                //Indsæt ChatEntity i databasen
                await connection.ExecuteAsync(
                    "INSERT INTO chats (id, name) VALUES (@Id, @Name)",
                    entity,
                    transaction);

                //indsæt ParticipationEntity i databasen
                await connection.ExecuteAsync(
                    "INSERT INTO participations (id, chat_id, participant_id, joined_at) VALUES (@Id, @ChatId, @ParticipantId, @JoinedAt)",
                    entity.Participations,
                    transaction);

                //hvis alt går godt, commit transaction
                await transaction.CommitAsync();
            }
            catch
            {
                //hvis noget går galt, rollback transaction
                await transaction.RollbackAsync();

                //kast fejlen videre, så handleren ved at chatten ikke blev oprettet
                throw;
            }
        }
    }
}
