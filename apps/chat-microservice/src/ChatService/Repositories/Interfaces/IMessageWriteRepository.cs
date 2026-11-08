using ChatService.DomainModel.Entities;

namespace ChatService.Repositories.Interfaces
{
    // Message er sit eget aggregate (refererer kun Chat med ChatId), derfor sit eget repository.
    public interface IMessageWriteRepository
    {
        // Gemmer én ny besked.
        Task AddAsync(Message message, CancellationToken cancellationToken);

        // Gemmer at en besked er slettet (soft delete, ADR 0020): tomt indhold og deleted_at.
        Task MarkDeletedAsync(Message message, CancellationToken cancellationToken);
    }
}
