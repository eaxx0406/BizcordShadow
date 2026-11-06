namespace ChatService.Contracts
{
    public record CreateChatRequest(string Name, Guid CreatedByUserId);
}
