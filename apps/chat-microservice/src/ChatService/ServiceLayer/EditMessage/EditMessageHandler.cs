using ChatService.DomainModel.Entities;
using ChatService.DomainModel.Exceptions;
using ChatService.Repositories.Interfaces;
using MediatR;

namespace ChatService.ServiceLayer.EditMessage
{
    // Samme mønster som DeleteMessageHandler: handleren henter og gemmer – Message afgør, om man må (Message.Edit).
    public class EditMessageHandler : IRequestHandler<EditMessageCommand>
    {
        private readonly IMessageReadRepository _messageReadRepository;
        private readonly IMessageWriteRepository _messageWriteRepository;

        public EditMessageHandler(IMessageReadRepository messageReadRepository, IMessageWriteRepository messageWriteRepository)
        {
            _messageReadRepository = messageReadRepository;
            _messageWriteRepository = messageWriteRepository;
        }

        public async Task Handle(EditMessageCommand command, CancellationToken cancellationToken)
        {
            //1. kald: hent beskeden (med afsenderens user-id)
            //findes den ikke, får man samme fejl, som hvis den ikke er ens egen (ADR 0013)
            Message message = await _messageReadRepository.GetByIdAsync(command.ChatId, command.MessageId, cancellationToken)
                ?? throw new ForbiddenException("Beskeden kunne ikke redigeres.");

            //domænet afgør, om brugeren må rette den, og om de 15 minutter er gået
            message.Edit(command.UserId, command.Content);

            //2. kald: gem den nye tekst og edited_at
            await _messageWriteRepository.MarkEditedAsync(message, cancellationToken);
        }
    }
}
