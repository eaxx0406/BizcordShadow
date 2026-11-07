using ChatService.Contracts;
using ChatService.DomainModel.Entities;
using ChatService.Repositories.Interfaces;
using Dapper;
using Npgsql;

namespace ChatService.Repositories
{
    public class ChatReadRepository: IChatReadRepository
    {
        private readonly string _connectionString;

        public ChatReadRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChatDb")
                                ?? throw new InvalidOperationException("Connection string 'ChatDb' mangler. Sæt den med dotnet user-secrets.");
        }
        public async Task<Chat?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            //Forbind til databasen
            await using var connection = new NpgsqlConnection(_connectionString);

            //Åbn forbindelsen
            await connection.OpenAsync(cancellationToken);

            //Hent chatten med Participants fra databasen
            //1) hent chatten
            ChatRow? chatRow = await connection.QuerySingleOrDefaultAsync<ChatRow>(new CommandDefinition(
                "SELECT id, name FROM chat WHERE id = @Id",
                new { Id = id },
                cancellationToken: cancellationToken));

            //return chatten, eller null hvis den ikke findes
            if (chatRow is null)
            {
                return null;
            }

            //2) hent kun de AKTIVE participations (left_at is null), se ADR 0010
            IEnumerable<ParticipationRow> participationRows = await connection.QueryAsync<ParticipationRow>(new CommandDefinition(
                "SELECT id, chat_id, participant_id, joined_at, left_at FROM participation WHERE chat_id = @ChatId AND left_at IS NULL",
                new { ChatId = id },
                cancellationToken: cancellationToken));

            //3) byg chatten gennem bagdøren (Rehydrate), så den beholder sit id fra databasen
            List<Participation> participations = participationRows
                .Select(row => Participation.Rehydrate(
                    row.Id,
                    row.ChatId,
                    row.ParticipantId,
                    new DateTimeOffset(row.JoinedAt),
                    row.LeftAt is null ? null : new DateTimeOffset(row.LeftAt.Value)))
                .ToList();

            return Chat.Rehydrate(chatRow.Id, chatRow.Name, participations);
        }

        public async Task<ChatDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            //Forbind til databasen
            await using var connection = new NpgsqlConnection(_connectionString);

            //Åbn forbindelsen
            await connection.OpenAsync(cancellationToken);

            //1) hent chatten
            ChatRow? chatRow = await connection.QuerySingleOrDefaultAsync<ChatRow>(new CommandDefinition(
                "SELECT id, name FROM chat WHERE id = @Id",
                new { Id = id },
                cancellationToken: cancellationToken));

            //return null, hvis chatten ikke findes (controlleren laver det til 404)
            if (chatRow is null)
            {
                return null;
            }

            //2) hent de AKTIVE medlemmer med navn: navnet står på participant, ikke på participation, derfor en JOIN
            //(AS UserId, så Dapper kan matche kolonnen med parameteren i ChatMemberDto's konstruktor)
            IEnumerable<ChatMemberDto> members = await connection.QueryAsync<ChatMemberDto>(new CommandDefinition(
                @"SELECT p.user_id AS UserId, p.name AS Name
                  FROM participation pa
                  JOIN participant p ON p.id = pa.participant_id
                  WHERE pa.chat_id = @ChatId AND pa.left_at IS NULL
                  ORDER BY pa.joined_at",
                new { ChatId = id },
                cancellationToken: cancellationToken));

            return new ChatDetailsDto(chatRow.Id, chatRow.Name, members.ToList());
        }
    }
}
