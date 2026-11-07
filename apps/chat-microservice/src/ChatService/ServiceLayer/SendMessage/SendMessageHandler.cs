using ChatService.DomainModel.Entities;
using ChatService.DomainModel.Exceptions;
using ChatService.Repositories.Interfaces;
using MediatR;

namespace ChatService.ServiceLayer.SendMessage
{
    public class SendMessageHandler : IRequestHandler<SendMessageCommand, Message>
    {
        private readonly IMessageWriteRepository _messageWriteRepository;
        private readonly IParticipationReadRepository _participationReadRepository;

        public SendMessageHandler(IMessageWriteRepository messageWriteRepository, IParticipationReadRepository participationReadRepository)
        {
            _messageWriteRepository = messageWriteRepository;
            _participationReadRepository = participationReadRepository;
        }

        public async Task<Message> Handle(SendMessageCommand command, CancellationToken cancellationToken)
        {
            // Afsenderen af en besked er en Participation (medlemskabet), ikke en Participant (se Message).
            // Oversæt derfor user-id (som klienten kender) til afsenderens AKTIVE participation i chatten.
            Guid? senderParticipationId = await _participationReadRepository.GetActiveIdAsync(command.ChatId, command.SenderUserId, cancellationToken);
            if (senderParticipationId == null)
            {
                // Kun medlemmer må skrive. Samme svar, uanset om chatten ikke findes eller man ikke er medlem (ADR 0013)
                throw new ForbiddenException("Du har ikke adgang til denne chat.");
            }

            // TODO (sammen): modtagernes MessageReceipts (tom besked afvises allerede med [Required] i SendMessageRequest)
            Message newMessage = new Message(command.ChatId, senderParticipationId.Value, command.Content);

            await _messageWriteRepository.AddAsync(newMessage, cancellationToken);
            return newMessage;
        }
    }
}
