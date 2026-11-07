using ChatService.Contracts;
using ChatService.DomainModel.Entities;

namespace ChatService.Repositories.Interfaces
{
    public interface IChatReadRepository
    {
        // Loader hele aggregatet (Chat + participations), så en command kan håndhæve reglerne.
        Task<Chat?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        // Til visning: SQL direkte til en DTO med medlemmernes navne, uden om domænemodellen (ADR 0004).
        // Returnerer null, hvis chatten ikke findes.
        Task<ChatDetailsDto?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken);

        // Til listen "mine chats": de chats, brugeren er aktivt medlem af, med seneste besked, nyeste aktivitet først.
        // Returnerer en tom liste, hvis brugeren ikke har nogen chats (eller ikke findes).
        Task<IReadOnlyList<ChatSummaryDto>> GetSummariesForUserAsync(Guid userId, CancellationToken cancellationToken);
    }
}
