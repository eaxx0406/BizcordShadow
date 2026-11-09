using ChatService.Contracts;
using ChatService.DomainModel.Entities;
using ChatService.Repositories.Interfaces;
using Dapper;
using Npgsql;

namespace ChatService.Repositories
{
    public class MessageReadRepository : IMessageReadRepository
    {
        private readonly string _connectionString;

        public MessageReadRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ChatDb")
                                ?? throw new InvalidOperationException("Connection string 'ChatDb' mangler. Sæt den med dotnet user-secrets.");
        }

        public async Task<MessagePageDto> GetPageAsync(Guid chatId, Guid? beforeMessageId, int limit, CancellationToken cancellationToken)
        {
            //Forbind til databasen
            await using var connection = new NpgsqlConnection(_connectionString);

            //Åbn forbindelsen
            await connection.OpenAsync(cancellationToken);

            //Keyset paging (ADR 0014):
            //- sorter NYESTE først og tag 'limit' rækker, så vi får de seneste beskeder
            //- "ældre end beskeden X" er (sent_at, id) < (X's sent_at, X's id). id er med, så to beskeder med
            //  præcis samme tidspunkt hverken springes over eller kommer med to gange
            //- vi henter limit + 1 rækker: kommer den ekstra række med, findes der ældre beskeder (HasOlder)
            string olderThan = beforeMessageId is null
                ? ""
                : "AND (m.sent_at, m.id) < (SELECT b.sent_at, b.id FROM message b WHERE b.id = @BeforeMessageId AND b.chat_id = @ChatId)";

            List<MessageRow> rows = (await connection.QueryAsync<MessageRow>(new CommandDefinition(
                $@"SELECT m.id, p.user_id AS sender_user_id, p.name AS sender_name, m.content, m.sent_at, m.deleted_at,
                          m.kind, m.system_type, m.details::text AS details
                   FROM message m
                   JOIN participation sp ON sp.id = m.sender_id
                   JOIN participant p ON p.id = sp.participant_id
                   WHERE m.chat_id = @ChatId {olderThan}
                   ORDER BY m.sent_at DESC, m.id DESC
                   LIMIT @Take",
                new { ChatId = chatId, BeforeMessageId = beforeMessageId, Take = limit + 1 },
                cancellationToken: cancellationToken))).ToList();

            bool hasOlder = rows.Count > limit;

            //Smid den ekstra række væk, og vend rækkefølgen, så den ældste kommer først (sådan vises de)
            List<ChatMessageDto> messages = rows
                .Take(limit)
                .Reverse()
                .Select(row => new ChatMessageDto(
                    row.Id,
                    row.SenderUserId,
                    row.SenderName,
                    row.Content,
                    new DateTimeOffset(row.SentAt),
                    IsDeleted: row.DeletedAt is not null,
                    row.Kind,
                    row.SystemType,
                    DetailsJson.Read(row.Details)))
                .ToList();

            return new MessagePageDto(messages, hasOlder);
        }

        public async Task<Message?> GetByIdAsync(Guid chatId, Guid messageId, CancellationToken cancellationToken)
        {
            //Forbind til databasen
            await using var connection = new NpgsqlConnection(_connectionString);

            //Åbn forbindelsen
            await connection.OpenAsync(cancellationToken);

            //Hent beskeden, men kun hvis den hører til netop denne chat (ellers kunne man ramme en besked i en anden chat).
            //Join til participant for at få afsenderens user-id, som Message.Delete skal bruge
            MessageAggregateRow? row = await connection.QuerySingleOrDefaultAsync<MessageAggregateRow>(new CommandDefinition(
                @"SELECT m.id, m.chat_id, m.sender_id, p.user_id AS sender_user_id, m.content, m.sent_at, m.deleted_at,
                         m.kind, m.system_type, m.details::text AS details
                  FROM message m
                  JOIN participation sp ON sp.id = m.sender_id
                  JOIN participant p ON p.id = sp.participant_id
                  WHERE m.id = @MessageId AND m.chat_id = @ChatId",
                new { ChatId = chatId, MessageId = messageId },
                cancellationToken: cancellationToken));

            if (row is null)
            {
                return null;
            }

            //Byg beskeden gennem bagdøren (Rehydrate), så den beholder sit id og tidspunkt fra databasen
            return Message.Rehydrate(
                row.Id,
                row.ChatId,
                row.SenderId,
                row.SenderUserId,
                row.Content,
                new DateTimeOffset(row.SentAt),
                row.DeletedAt is null ? null : new DateTimeOffset(row.DeletedAt.Value),
                row.Kind,
                row.SystemType,
                DetailsJson.Read(row.Details));
        }
    }
}
