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

        public async Task CreateAsync(Chat chat, CancellationToken cancellationToken)
        {
            //forbinde til databasen
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            //start en new transaction
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            try
            {
                //Indsæt ChatEntity i databasen
                //(CommandDefinition er Dappers måde at få både transaction og cancellationToken med)
                await connection.ExecuteAsync(new CommandDefinition(
                    "INSERT INTO chat (id, name) VALUES (@Id, @Name)",
                    chat,
                    transaction,
                    cancellationToken: cancellationToken));

                //indsæt ParticipationEntity i databasen
                await connection.ExecuteAsync(new CommandDefinition(
                    "INSERT INTO participation (id, chat_id, participant_id, joined_at) VALUES (@Id, @ChatId, @ParticipantId, @JoinedAt)",
                    chat.Participations,
                    transaction,
                    cancellationToken: cancellationToken));

                //hvis alt går godt, commit transaction
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                //hvis noget går galt, rollback transaction
                //(uden cancellationToken: rollback skal gennemføres, også hvis requesten er afbrudt)
                await transaction.RollbackAsync();

                //kast fejlen videre, så handleren ved at chatten ikke blev oprettet
                throw;
            }
        }

        public async Task AddParticipationAsync(Participation participation, CancellationToken cancellationToken)
        {
            //forbinde til databasen
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            //indsæt den nye ParticipationEntity i databasen
            //(ingen transaction: det er kun én INSERT, og én SQL-sætning er altid alt-eller-intet)
            await connection.ExecuteAsync(new CommandDefinition(
                "INSERT INTO participation (id, chat_id, participant_id, joined_at) VALUES (@Id, @ChatId, @ParticipantId, @JoinedAt)",
                participation,
                cancellationToken: cancellationToken));
        }
    }
}
