using ChatService.DomainModel.Entities;
using ChatService.DomainModel.Exceptions;
using ChatService.Repositories.Interfaces;
using MediatR;

namespace ChatService.ServiceLayer.LeaveChat
{
    public class LeaveChatHandler : IRequestHandler<LeaveChatCommand>
    {
        private readonly IChatWriteRepository _chatWriteRepository;
        private readonly IChatReadRepository _chatReadRepository;
        private readonly IParticipantReadRepository _participantReadRepository;

        public LeaveChatHandler(IChatWriteRepository chatWriteRepository, IChatReadRepository chatReadRepository, IParticipantReadRepository participantReadRepository)
        {
            _chatWriteRepository = chatWriteRepository;
            _chatReadRepository = chatReadRepository;
            _participantReadRepository = participantReadRepository;
        }

        public async Task Handle(LeaveChatCommand command, CancellationToken cancellationToken)
        {
            // 1) Hent aggregatet. Findes chatten ikke, er svaret det samme som "ikke medlem" (ADR 0013)
            Chat chat = await _chatReadRepository.GetByIdAsync(command.ChatId, cancellationToken)
                ?? throw new ForbiddenException("Du har ikke adgang til denne chat.");

            // 2) Oversæt user-id (som klienten kender) til participant-id (som ChatService bruger), se ADR 0003.
            //    En ukendt bruger kan heller ikke være medlem, så også her: "ingen adgang"
            Guid participantId = await _participantReadRepository.GetIdByUserIdAsync(command.UserId, cancellationToken)
                ?? throw new ForbiddenException("Du har ikke adgang til denne chat.");

            // 3) Domænet beslutter: reglerne ligger i Chat.Leave og MembershipPeriod, ikke her
            Participation endedParticipation = chat.Leave(participantId);

            // 4) Gem kun det, der er ændret: den ene afsluttede participation
            await _chatWriteRepository.LeaveParticipationAsync(endedParticipation, cancellationToken);
        }
    }
}
