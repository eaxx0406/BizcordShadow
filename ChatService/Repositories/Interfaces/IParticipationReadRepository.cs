namespace ChatService.Repositories.Interfaces
{
    public interface IParticipationReadRepository
    {
        // Finder id'et på brugerens AKTIVE participation (medlemskab) i chatten.
        // Returnerer null, hvis brugeren ikke findes eller ikke er aktivt medlem af chatten.
        Task<Guid?> GetActiveIdAsync(Guid chatId, Guid userId, CancellationToken cancellationToken);
    }
}
