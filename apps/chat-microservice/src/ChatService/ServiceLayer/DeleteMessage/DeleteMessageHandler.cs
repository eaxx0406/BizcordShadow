using ChatService.DomainModel.Entities;
using ChatService.DomainModel.Exceptions;
using ChatService.Repositories.Interfaces;
using MediatR;

namespace ChatService.ServiceLayer.DeleteMessage
{
    public class DeleteMessageHandler : IRequestHandler<DeleteMessageCommand>
    {
        private readonly IMessageReadRepository _messageReadRepository;
        private readonly IMessageWriteRepository _messageWriteRepository;
        private readonly IParticipationReadRepository _participationReadRepository;

        public DeleteMessageHandler(IMessageReadRepository messageReadRepository, IMessageWriteRepository messageWriteRepository, IParticipationReadRepository participationReadRepository)
        {
            _messageReadRepository = messageReadRepository;
            _messageWriteRepository = messageWriteRepository;
            _participationReadRepository = participationReadRepository;
        }

        public async Task Handle(DeleteMessageCommand command, CancellationToken cancellationToken)
        {
            // 1) Kun aktive medlemmer. Samme svar, uanset om chatten ikke findes eller man ikke er med (ADR 0013)
            Guid participationId = await _participationReadRepository.GetActiveIdAsync(command.ChatId, command.UserId, cancellationToken)
                ?? throw new ForbiddenException("Du har ikke adgang til denne chat.");

            // 2) Hent beskeden. Man ER medlem, så det afslører intet at sige, at beskeden ikke findes
            Message message = await _messageReadRepository.GetByIdAsync(command.ChatId, command.MessageId, cancellationToken)
                ?? throw new NotFoundException("Beskeden findes ikke.");

            // 3) Domænet beslutter: kun afsenderen må slette (Message.Delete)
            message.Delete(participationId);

            // 4) Gem: tomt indhold og deleted_at
            await _messageWriteRepository.MarkDeletedAsync(message, cancellationToken);
        }
    }
}
