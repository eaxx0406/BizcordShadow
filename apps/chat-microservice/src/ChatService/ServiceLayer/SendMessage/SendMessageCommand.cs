using ChatService.DomainModel.Entities;
using MediatR;

namespace ChatService.ServiceLayer.SendMessage
{
    public record SendMessageCommand(Guid ChatId, Guid SenderUserId, string Content) : IRequest<Message>;
}
