using ChatService.Contracts;
using ChatService.DomainModel.Entities;

namespace ChatService.Repositories.Interfaces
{
    public interface IMessageReadRepository
    {
        // Henter højst 'limit' beskeder fra chatten, ældste først (keyset paging, ADR 0014).
        // beforeMessageId = null: de nyeste. Ellers: kun beskeder, der er ældre end den besked.
        Task<MessagePageDto> GetPageAsync(Guid chatId, Guid? beforeMessageId, int limit, CancellationToken cancellationToken);

        // Henter én besked som aggregat (til at ændre den). null, hvis den ikke findes i netop den chat.
        Task<Message?> GetByIdAsync(Guid chatId, Guid messageId, CancellationToken cancellationToken);
    }
}
