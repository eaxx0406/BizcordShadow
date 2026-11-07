using ChatService.Contracts;
using MediatR;

namespace ChatService.ServiceLayer.GetMyChats
{
    // Query (læs): alle chats, som brugeren er aktivt medlem af, nyeste aktivitet først.
    // UserId kommer fra klienten, indtil der findes login (så kommer det fra ICurrentUser).
    public record GetMyChatsQuery(Guid UserId) : IRequest<IReadOnlyList<ChatSummaryDto>>;
}
