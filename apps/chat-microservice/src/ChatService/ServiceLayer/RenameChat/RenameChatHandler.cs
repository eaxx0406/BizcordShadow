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
        private readonly IMessageWriteRepository _messageWriteRepository;

        public RenameChatHandler(IChatWriteRepository chatWriteRepository, IChatReadRepository chatReadRepository, IParticipantReadRepository participantReadRepository, IMessageWriteRepository messageWriteRepository)
        {
            _chatWriteRepository = chatWriteRepository;
            _chatReadRepository = chatReadRepository;
            _participantReadRepository = participantReadRepository;
            _messageWriteRepository = messageWriteRepository;
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
            //    (det gamle navn huskes til systembeskeden)
            string oldName = chat.Name;
            Participation renamedBy = chat.Rename(participantId, command.Name);

            // 4) Gem kun det, der er ændret: navnet
            await _chatWriteRepository.UpdateNameAsync(chat, cancellationToken);

            // 5) Systembesked i tidslinjen: "Pia omdøbte chatten til ..." (ADR 0022).
            //    Gemmes EFTER ændringen og ikke i samme transaction: fejler den, mangler der kun en linje i tidslinjen
            //    (samme navn som før: intet skete, så ingen systembesked)
            if (oldName != chat.Name)
            {
                await _messageWriteRepository.AddAsync(Message.ChatRenamed(chat.Id, renamedBy.Id, command.UserId, oldName, chat.Name), cancellationToken);
            }
        }
    }
}
