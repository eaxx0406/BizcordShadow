namespace ChatService.Repositories.Interfaces
{
    public interface IParticipationReadRepository
    {
        // Finder id'et på brugerens AKTIVE participation (medlemskab) i chatten.
        // Returnerer null, hvis brugeren ikke findes eller ikke er aktivt medlem af chatten.
        Task<Guid?> GetActiveIdAsync(Guid chatId, Guid userId, CancellationToken cancellationToken);

        // Finder user-id'erne på alle AKTIVE deltagere i chatten (dem, der ikke har forladt den).
        Task<List<Guid>> GetActiveUserIdsAsync(Guid chatId, CancellationToken cancellationToken);
    }
}
