using MediatR;

namespace ChatService.ServiceLayer.EditMessage
{
    // Brugeren (UserId) retter sin egen besked (MessageId) i chatten (ChatId) til Content.
    public record EditMessageCommand(Guid ChatId, Guid MessageId, Guid UserId, string Content) : IRequest;
}
