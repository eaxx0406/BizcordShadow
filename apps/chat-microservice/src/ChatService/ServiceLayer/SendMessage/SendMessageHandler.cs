using ChatService.DomainModel.Entities;
using ChatService.DomainModel.Exceptions;
using ChatService.Repositories.Interfaces;
using ChatService.Contracts.Events;
using MediatR;
using MessageClient.Interfaces;

namespace ChatService.ServiceLayer.SendMessage
{
    public class SendMessageHandler : IRequestHandler<SendMessageCommand, Message>
    {
        private readonly IMessageWriteRepository _messageWriteRepository;
        private readonly IParticipationReadRepository _participationReadRepository;
        private readonly IMessageClient _messageClient;

        public SendMessageHandler(IMessageWriteRepository messageWriteRepository, IParticipationReadRepository participationReadRepository, IMessageClient messageClient)
        {
            _messageWriteRepository = messageWriteRepository;
            _participationReadRepository = participationReadRepository;
            _messageClient = messageClient;
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
            Message newMessage = new Message(command.ChatId, senderParticipationId.Value, command.SenderUserId, command.Content);

            await _messageWriteRepository.AddAsync(newMessage, cancellationToken);

            // Modtagerne = de aktive deltagere lige nu, minus afsenderen selv.
            // ChatService afgør det her og nu, så listen passer til netop denne besked (se MessageSent).
            List<Guid> activeUserIds = await _participationReadRepository.GetActiveUserIdsAsync(command.ChatId, cancellationToken);
            List<Guid> recipientUserIds = activeUserIds.Where(userId => userId != command.SenderUserId).ToList();

            // Fortæl resten af systemet, at beskeden er sendt. EFTER den er gemt: vi må ikke
            // melde noget ud, som så alligevel ikke står i databasen.
            await _messageClient.Publish(new MessageSent(
                newMessage.Id, newMessage.ChatId, command.SenderUserId, recipientUserIds, newMessage.Content, newMessage.SentAt));

            return newMessage;
        }
    }
}
