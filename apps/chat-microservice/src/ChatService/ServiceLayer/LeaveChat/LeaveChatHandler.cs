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
        private readonly IMessageWriteRepository _messageWriteRepository;

        public LeaveChatHandler(IChatWriteRepository chatWriteRepository, IChatReadRepository chatReadRepository, IParticipantReadRepository participantReadRepository, IMessageWriteRepository messageWriteRepository)
        {
            _chatWriteRepository = chatWriteRepository;
            _chatReadRepository = chatReadRepository;
            _participantReadRepository = participantReadRepository;
            _messageWriteRepository = messageWriteRepository;
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

            // 5) Systembesked i tidslinjen: "Kasper forlod chatten" (ADR 0022).
            //    Gemmes EFTER ændringen og ikke i samme transaction: fejler den, mangler der kun en linje i tidslinjen
            await _messageWriteRepository.AddAsync(Message.MemberLeft(chat.Id, endedParticipation.Id, command.UserId), cancellationToken);
        }
    }
}
