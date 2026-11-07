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
            // Oversæt user-id'er (som klienten kender) til participant-id'er (som ChatService bruger), se ADR 0003
            Guid creatorParticipantId = await GetParticipantIdAsync(command.CreatedByUserId, cancellationToken);

            List<Guid> memberParticipantIds = new();
            foreach (Guid userId in command.ParticipantUserIds.Distinct())
            {
                memberParticipantIds.Add(await GetParticipantIdAsync(userId, cancellationToken));
            }

            // Chat håndhæver reglerne: mindst ét andet medlem, ingen dubletter, maks 50 (ADR 0010 og 0012)
            Chat newChat = Chat.Create(command.Name, creatorParticipantId, memberParticipantIds);

            // Chatten og alle participations gemmes i én transaktion: enten findes chatten med alle medlemmer, eller slet ikke
            await _chatWriteRepository.CreateAsync(newChat, cancellationToken);
            return newChat;
        }

        private async Task<Guid> GetParticipantIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            Guid? participantId = await _participantReadRepository.GetIdByUserIdAsync(userId, cancellationToken);
            if (participantId == null)
            {
                throw new Exception($"User with ID {userId} not found.");
            }
            return participantId.Value;
        }
    }
}
