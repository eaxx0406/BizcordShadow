using ChatService.Contracts.Events;
using Dapper;
using Npgsql;

namespace RealTimeCommunicationService.Repositories
{
    public class ReceivedMessageRepository
    {
        private readonly string _connectionString;

        public ReceivedMessageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("RealTimeDb")
                                ?? throw new InvalidOperationException("Connection string 'RealTimeDb' mangler. Se README.");
        }

        public async Task AddAsync(MessageSent message)
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            // on conflict do nothing: kommer samme event igen, skriver vi det ikke to gange.
            await connection.ExecuteAsync(
                @"INSERT INTO received_message (message_id, chat_id, sender_user_id, sent_at, received_at)
                  VALUES (@MessageId, @ChatId, @SenderUserId, @SentAt, @ReceivedAt)
                  ON CONFLICT (message_id) DO NOTHING",
                new { message.MessageId, message.ChatId, message.SenderUserId, message.SentAt, ReceivedAt = DateTimeOffset.UtcNow });
        }

        public async Task<IEnumerable<ReceivedMessageRow>> GetAllAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);

            return await connection.QueryAsync<ReceivedMessageRow>(
                "SELECT message_id, chat_id, sender_user_id, sent_at, received_at FROM received_message ORDER BY received_at DESC");
        }
    }
}
