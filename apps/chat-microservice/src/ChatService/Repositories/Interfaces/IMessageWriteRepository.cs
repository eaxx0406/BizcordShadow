using ChatService.DomainModel.Entities;

namespace ChatService.Repositories.Interfaces
{
    // Message er sit eget aggregate (refererer kun Chat med ChatId), derfor sit eget repository.
    public interface IMessageWriteRepository
    {
        // Gemmer én ny besked.
        Task AddAsync(Message message, CancellationToken cancellationToken);

        // Gemmer en slettet besked (tom tekst og deleted_at, sat af Message.Delete).
        Task MarkDeletedAsync(Message message, CancellationToken cancellationToken);
    }
}
