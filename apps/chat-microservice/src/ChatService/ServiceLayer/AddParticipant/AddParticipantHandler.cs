using ChatService.DomainModel.Entities;
using ChatService.DomainModel.Exceptions;
using ChatService.Repositories.Interfaces;
using MediatR;

namespace ChatService.ServiceLayer.AddParticipant
{
    public class AddParticipantHandler: IRequestHandler<AddParticipantCommand>
    {
        private readonly IChatWriteRepository _chatWriteRepository;
        private readonly IChatReadRepository _chatReadRepository;
        private readonly IParticipantReadRepository _participantReadRepository;
        private readonly IMessageWriteRepository _messageWriteRepository;
        public AddParticipantHandler(IChatWriteRepository chatWriteRepository, IChatReadRepository chatReadRepository, IParticipantReadRepository participantReadRepository, IMessageWriteRepository messageWriteRepository)
        {
            _chatWriteRepository = chatWriteRepository;
            _chatReadRepository = chatReadRepository;
            _participantReadRepository = participantReadRepository;
            _messageWriteRepository = messageWriteRepository;
        }
        public async Task Handle(AddParticipantCommand command, CancellationToken cancellationToken)
        {
            // Retrieve the chat from the repository (not shown here, assuming you have a method for that)
            Chat? chat = await _chatReadRepository.GetByIdAsync(command.ChatId, cancellationToken);
            if (chat == null)
            {
                throw new NotFoundException("Chatten findes ikke.");
            }
            // Oversæt user-id (som klienten kender) til participant-id (som ChatService bruger), se ADR 0003
            Guid? participantId = await _participantReadRepository.GetIdByUserIdAsync(command.UserId, cancellationToken);
            if (participantId == null)
            {
                throw new NotFoundException("Brugeren findes ikke.");
            }
            // Add the participant to the chat
            Participation newParticipation = chat.AddParticipant(participantId.Value);
            // Save the updated chat back to the repository
            await _chatWriteRepository.AddParticipationAsync(newParticipation, cancellationToken); // enten det eller så skal vi have en decideret AddParticipantAsync metode i repositoryet, som kun opdaterer participants listen

            // Systembesked i tidslinjen: "Oliver blev tilføjet" (ADR 0022). Afsenderen er den nye participation,
            // fordi requesten ikke siger, hvem der tilføjede. Gemmes EFTER ændringen og ikke i samme transaction:
            // fejler den, mangler der kun en linje i tidslinjen
            await _messageWriteRepository.AddAsync(Message.MemberAdded(chat.Id, newParticipation.Id, command.UserId), cancellationToken);
        }
    }
}
