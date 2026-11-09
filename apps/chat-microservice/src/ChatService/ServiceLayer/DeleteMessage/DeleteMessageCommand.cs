using MediatR;

namespace ChatService.ServiceLayer.DeleteMessage
{
    // Brugeren (UserId) sletter sin egen besked (MessageId) i chatten (ChatId).
    public record DeleteMessageCommand(Guid ChatId, Guid MessageId, Guid UserId) : IRequest;
}
