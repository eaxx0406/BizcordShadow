using ChatService.Contracts;
using ChatService.DomainModel.Exceptions;
using ChatService.Repositories.Interfaces;
using MediatR;

namespace ChatService.ServiceLayer.GetChatById
{
    public class GetChatByIdHandler : IRequestHandler<GetChatByIdQuery, ChatDetailsDto>
    {
        private readonly IChatReadRepository _chatReadRepository;
        private readonly IParticipationReadRepository _participationReadRepository;

        public GetChatByIdHandler(IChatReadRepository chatReadRepository, IParticipationReadRepository participationReadRepository)
        {
            _chatReadRepository = chatReadRepository;
            _participationReadRepository = participationReadRepository;
        }

        public async Task<ChatDetailsDto> Handle(GetChatByIdQuery query, CancellationToken cancellationToken)
        {
            // Kun medlemmer må se chatten. Samme svar, uanset om chatten ikke findes eller man ikke er medlem (ADR 0013)
            Guid? participationId = await _participationReadRepository.GetActiveIdAsync(query.ChatId, query.UserId, cancellationToken);
            if (participationId == null)
            {
                throw new ForbiddenException("Du har ikke adgang til denne chat.");
            }

            // Ingen domænemodel og ingen regler: en query henter bare data i den form, klienten skal bruge (ADR 0004).
            // Er man medlem, findes chatten, så null sker kun, hvis den bliver slettet imens.
            return await _chatReadRepository.GetDetailsByIdAsync(query.ChatId, cancellationToken)
                ?? throw new ForbiddenException("Du har ikke adgang til denne chat.");
        }
    }
}
