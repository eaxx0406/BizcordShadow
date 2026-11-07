using ChatService.Contracts;
using ChatService.Repositories.Interfaces;
using MediatR;

namespace ChatService.ServiceLayer.GetMyChats
{
    public class GetMyChatsHandler : IRequestHandler<GetMyChatsQuery, IReadOnlyList<ChatSummaryDto>>
    {
        private readonly IChatReadRepository _chatReadRepository;

        public GetMyChatsHandler(IChatReadRepository chatReadRepository)
        {
            _chatReadRepository = chatReadRepository;
        }

        public async Task<IReadOnlyList<ChatSummaryDto>> Handle(GetMyChatsQuery query, CancellationToken cancellationToken)
        {
            // En ukendt bruger er ikke en fejl her: vedkommende har bare ingen chats (tom liste)
            return await _chatReadRepository.GetSummariesForUserAsync(query.UserId, cancellationToken);
        }
    }
}
