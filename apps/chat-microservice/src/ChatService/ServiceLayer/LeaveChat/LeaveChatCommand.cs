using MediatR;

namespace ChatService.ServiceLayer.LeaveChat
{
    // Brugeren (UserId) forlader selv chatten (ChatId). Man kan kun fjerne sig selv, ikke andre.
    public record LeaveChatCommand(Guid ChatId, Guid UserId) : IRequest;
}
