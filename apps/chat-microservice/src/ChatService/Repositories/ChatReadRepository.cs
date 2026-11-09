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

        public async Task<IReadOnlyList<ChatSummaryDto>> GetSummariesForUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            //Forbind til databasen
            await using var connection = new NpgsqlConnection(_connectionString);

            //Åbn forbindelsen
            await connection.OpenAsync(cancellationToken);

            //Én query for hele listen:
            //- me/mine: find brugerens participant og de chats, vedkommende er AKTIVT medlem af
            //- participant_count: antal aktive medlemmer (bruges til at vise gruppe- eller person-ikon)
            //- LEFT JOIN LATERAL: for HVER chat køres underqueryen, som finder den nyeste besked (LIMIT 1).
            //  LATERAL betyder, at underqueryen må bruge c.id fra rækken udenfor.
            //  LEFT ... ON true: chatten kommer med, selvom den ikke har nogen beskeder (så er lm-kolonnerne null).
            //- sortering: nyeste besked først; uden beskeder bruges tidspunktet, hvor brugeren blev medlem
            IEnumerable<ChatSummaryRow> rows = await connection.QueryAsync<ChatSummaryRow>(new CommandDefinition(
                @"SELECT c.id,
                         c.name,
                         (SELECT count(*)::int FROM participation x WHERE x.chat_id = c.id AND x.left_at IS NULL) AS participant_count,
                         lm.content     AS last_message,
                         lm.sender_name AS last_message_sender_name,
                         lm.sent_at     AS last_message_at,
                         lm.system_type AS last_message_system_type
                  FROM participant me
                  JOIN participation mine ON mine.participant_id = me.id AND mine.left_at IS NULL
                  JOIN chat c ON c.id = mine.chat_id
                  LEFT JOIN LATERAL (
                      SELECT CASE WHEN m.deleted_at IS NULL THEN m.content END AS content, -- null, hvis den er slettet (ADR 0020)
                             p.name AS sender_name, m.sent_at,
                             m.system_type -- sat for systembeskeder (ADR 0022)
                      FROM message m
                      JOIN participation sp ON sp.id = m.sender_id
                      JOIN participant p ON p.id = sp.participant_id
                      WHERE m.chat_id = c.id
                      ORDER BY m.sent_at DESC
                      LIMIT 1
                  ) lm ON true
                  WHERE me.user_id = @UserId
                  ORDER BY COALESCE(lm.sent_at, mine.joined_at) DESC",
                new { UserId = userId },
                cancellationToken: cancellationToken));

            return rows
                .Select(row => new ChatSummaryDto(
                    row.Id,
                    row.Name,
                    row.ParticipantCount,
                    row.LastMessage,
                    row.LastMessageSenderName,
                    row.LastMessageAt is null ? null : new DateTimeOffset(row.LastMessageAt.Value),
                    row.LastMessageSystemType))
                .ToList();
        }
    }
}
