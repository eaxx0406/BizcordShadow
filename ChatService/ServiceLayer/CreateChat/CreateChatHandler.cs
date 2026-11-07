using System.Reflection.Metadata.Ecma335;
using MediatR;
using ChatService.DomainModel.Entities;
using ChatService.Repositories.Interfaces;

namespace ChatService.ServiceLayer.CreateChat
{
    public class CreateChatHandler:IRequestHandler<CreateChatCommand, Chat>
    {
        private readonly IChatWriteRepository _chatWriteRepository;
        private readonly IParticipantReadRepository _participantReadRepository;

        public CreateChatHandler(IChatWriteRepository chatWriteRepository, IParticipantReadRepository participantReadRepository)
        {
            _chatWriteRepository = chatWriteRepository;
            _participantReadRepository = participantReadRepository;
        }

        public async Task<Chat> Handle(CreateChatCommand command, CancellationToken cancellationToken)
        {
            // Oversæt user-id (som klienten kender) til participant-id (som ChatService bruger), se ADR 0003
            Guid? creatorParticipantId = await _participantReadRepository.GetIdByUserIdAsync(command.CreatedByUserId, cancellationToken);
            if (creatorParticipantId == null)
            {
                throw new Exception($"User with ID {command.CreatedByUserId} not found.");
            }

            Chat newChat = new Chat(command.Name);
            newChat.AddParticipant(creatorParticipantId.Value);

            await _chatWriteRepository.CreateAsync(newChat, cancellationToken);
            return newChat;
        }
    }
}
