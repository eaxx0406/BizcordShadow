using ChatService.DomainModel.Entities;
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
                // TODO (sammen): hvilken regel er det her, og hvor hører den hjemme? Og hvilken statuskode skal klienten have?
                throw new Exception($"User with ID {command.SenderUserId} is not an active member of chat {command.ChatId}.");
            }

            // TODO (sammen): regler for indholdet (tomt, maks længde) og modtagernes MessageReceipts
            Message newMessage = new Message(command.ChatId, senderParticipationId.Value, command.Content);

            await _messageWriteRepository.AddAsync(newMessage, cancellationToken);
            return newMessage;
        }
    }
}
