namespace ChatService.Contracts
{
    // SenderUserId sendes af klienten, indtil der findes login (så kommer det fra ICurrentUser).
    public record SendMessageRequest(Guid SenderUserId, string Content);
}
