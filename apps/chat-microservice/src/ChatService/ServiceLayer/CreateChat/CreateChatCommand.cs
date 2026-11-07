using ChatService.DomainModel.Entities;
using MediatR;

namespace ChatService.ServiceLayer.CreateChat
{
    public record CreateChatCommand(string Name, Guid CreatedByUserId, IReadOnlyList<Guid> ParticipantUserIds) : IRequest<Chat>;
}
