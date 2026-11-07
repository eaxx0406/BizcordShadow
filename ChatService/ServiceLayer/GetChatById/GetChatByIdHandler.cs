using ChatService.Contracts;
using ChatService.Repositories.Interfaces;
using MediatR;

namespace ChatService.ServiceLayer.GetChatById
{
    public class GetChatByIdHandler : IRequestHandler<GetChatByIdQuery, ChatDetailsDto?>
    {
        private readonly IChatReadRepository _chatReadRepository;

        public GetChatByIdHandler(IChatReadRepository chatReadRepository)
        {
            _chatReadRepository = chatReadRepository;
        }

        public async Task<ChatDetailsDto?> Handle(GetChatByIdQuery query, CancellationToken cancellationToken)
        {
            // Ingen domænemodel og ingen regler: en query henter bare data i den form, klienten skal bruge (ADR 0004)
            return await _chatReadRepository.GetDetailsByIdAsync(query.ChatId, cancellationToken);
        }
    }
}
