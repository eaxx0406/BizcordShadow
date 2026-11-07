namespace ChatService.Repositories.Interfaces
{
    // Participant er en read model af en User fra UserService (se ADR 0003) og sit eget aggregate,
    // derfor sit eget repository. Write-delen kommer, når der findes events fra UserService.
    public interface IParticipantReadRepository
    {
        // Oversætter et user-id (som klienten kender) til ChatService' eget participant-id.
        // Returnerer null, hvis brugeren ikke findes i ChatService.
        Task<Guid?> GetIdByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
