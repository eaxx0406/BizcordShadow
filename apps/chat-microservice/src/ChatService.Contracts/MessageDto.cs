namespace ChatService.Contracts
{
    public record MessageDto(Guid Id, Guid ChatId, string Content, DateTimeOffset SentAt);
}
