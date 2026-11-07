using ChatService.Contracts;
using MediatR;

namespace ChatService.ServiceLayer.GetMessages
{
    // Query (læs): én side beskeder fra en chat (keyset paging, ADR 0014).
    // BeforeMessageId = null: de nyeste. Ellers: dem, der er ældre end den besked.
    public record GetMessagesQuery(Guid ChatId, Guid UserId, Guid? BeforeMessageId, int Limit) : IRequest<MessagePageDto>;
}
