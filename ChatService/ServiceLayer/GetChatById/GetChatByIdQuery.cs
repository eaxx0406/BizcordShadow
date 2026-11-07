using ChatService.Contracts;
using MediatR;

namespace ChatService.ServiceLayer.GetChatById
{
    // Query (læs): ændrer ingenting. Returnerer null, hvis chatten ikke findes.
    public record GetChatByIdQuery(Guid ChatId) : IRequest<ChatDetailsDto?>;
}
