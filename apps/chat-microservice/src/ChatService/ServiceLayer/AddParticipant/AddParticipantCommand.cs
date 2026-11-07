using MediatR;

namespace ChatService.ServiceLayer.AddParticipant
{
    public record AddParticipantCommand(Guid ChatId, Guid UserId) : IRequest;
}
