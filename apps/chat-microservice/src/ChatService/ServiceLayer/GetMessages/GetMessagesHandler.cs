using ChatService.Contracts;
using ChatService.DomainModel.Exceptions;
using ChatService.Repositories.Interfaces;
using MediatR;

namespace ChatService.ServiceLayer.GetMessages
{
    public class GetMessagesHandler : IRequestHandler<GetMessagesQuery, MessagePageDto>
    {
        private readonly IMessageReadRepository _messageReadRepository;
        private readonly IParticipationReadRepository _participationReadRepository;

        public GetMessagesHandler(IMessageReadRepository messageReadRepository, IParticipationReadRepository participationReadRepository)
        {
            _messageReadRepository = messageReadRepository;
            _participationReadRepository = participationReadRepository;
        }

        public async Task<MessagePageDto> Handle(GetMessagesQuery query, CancellationToken cancellationToken)
        {
            // Kun medlemmer må læse. Samme svar, uanset om chatten ikke findes eller man ikke er medlem (ADR 0013)
            Guid? participationId = await _participationReadRepository.GetActiveIdAsync(query.ChatId, query.UserId, cancellationToken);
            if (participationId == null)
            {
                throw new ForbiddenException("Du har ikke adgang til denne chat.");
            }

            return await _messageReadRepository.GetPageAsync(query.ChatId, query.BeforeMessageId, query.Limit, cancellationToken);
        }
    }
}
