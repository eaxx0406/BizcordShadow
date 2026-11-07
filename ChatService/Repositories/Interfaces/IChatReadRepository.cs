using ChatService.DomainModel.Entities;

namespace ChatService.Repositories.Interfaces
{
    public interface IChatReadRepository
    {
        Task<Chat?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
