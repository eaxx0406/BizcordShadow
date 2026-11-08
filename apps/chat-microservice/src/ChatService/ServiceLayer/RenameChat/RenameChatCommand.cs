using MediatR;

namespace ChatService.ServiceLayer.RenameChat
{
    // Brugeren (UserId) giver chatten (ChatId) et nyt navn.
    public record RenameChatCommand(Guid ChatId, Guid UserId, string Name) : IRequest;
}
