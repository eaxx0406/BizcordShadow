using ChatService.DomainModel.Entities;
using ChatService.DomainModel.Exceptions;
using ChatService.Repositories.Interfaces;
using MediatR;

namespace ChatService.ServiceLayer.DeleteMessage
{
    // Normal sletning (ADR 0021). Handleren henter og gemmer – Message afgør, om man må (Message.Delete).
    public class DeleteMessageHandler : IRequestHandler<DeleteMessageCommand>
    {
        private readonly IMessageReadRepository _messageReadRepository;
        private readonly IMessageWriteRepository _messageWriteRepository;

        public DeleteMessageHandler(IMessageReadRepository messageReadRepository, IMessageWriteRepository messageWriteRepository)
        {
            _messageReadRepository = messageReadRepository;
            _messageWriteRepository = messageWriteRepository;
        }

        public async Task Handle(DeleteMessageCommand command, CancellationToken cancellationToken)
        {
            //1. kald: hent beskeden (med afsenderens user-id)
            //findes den ikke, får man samme fejl, som hvis den ikke er ens egen (ADR 0013)
            Message message = await _messageReadRepository.GetByIdAsync(command.ChatId, command.MessageId, cancellationToken)
                ?? throw new ForbiddenException("Beskeden kunne ikke slettes.");

            //domænet afgør, om brugeren må slette den (kaster ForbiddenException ellers)
            message.Delete(command.UserId);

            //2. kald: gem tom tekst og deleted_at
            await _messageWriteRepository.MarkDeletedAsync(message, cancellationToken);
        }
    }
}
