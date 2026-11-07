using ChatService.Contracts;
using MediatR;

namespace ChatService.ServiceLayer.GetChatById
{
    // Query (læs): ændrer ingenting. Kun medlemmer må se chatten (ADR 0013).
    // UserId kommer fra klienten, indtil der findes login.
    public record GetChatByIdQuery(Guid ChatId, Guid UserId) : IRequest<ChatDetailsDto>;
}
