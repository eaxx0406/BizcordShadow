using ChatService.DomainModel.Entities;
using ChatService.DomainModel.Exceptions;
using ChatService.Repositories.Interfaces;
using MediatR;

namespace ChatService.ServiceLayer.RenameChat
{
    public class RenameChatHandler : IRequestHandler<RenameChatCommand>
    {
        private readonly IChatWriteRepository _chatWriteRepository;
        private readonly IChatReadRepository _chatReadRepository;
        private readonly IParticipantReadRepository _participantReadRepository;

        public RenameChatHandler(IChatWriteRepository chatWriteRepository, IChatReadRepository chatReadRepository, IParticipantReadRepository participantReadRepository)
        {
            _chatWriteRepository = chatWriteRepository;
            _chatReadRepository = chatReadRepository;
            _participantReadRepository = participantReadRepository;
        }

        public async Task Handle(RenameChatCommand command, CancellationToken cancellationToken)
        {
            // 1) Hent aggregatet. Findes chatten ikke, er svaret det samme som "ikke medlem" (ADR 0013)
            Chat chat = await _chatReadRepository.GetByIdAsync(command.ChatId, cancellationToken)
                ?? throw new ForbiddenException("Du har ikke adgang til denne chat.");

            // 2) Oversæt user-id til participant-id (ADR 0003). En ukendt bruger kan heller ikke være medlem
            Guid participantId = await _participantReadRepository.GetIdByUserIdAsync(command.UserId, cancellationToken)
                ?? throw new ForbiddenException("Du har ikke adgang til denne chat.");

            // 3) Domænet beslutter: kun aktive medlemmer, og navnet skal være gyldigt (Chat.Rename)
            chat.Rename(participantId, command.Name);

            // 4) Gem kun det, der er ændret: navnet
            await _chatWriteRepository.UpdateNameAsync(chat, cancellationToken);
        }
    }
}
