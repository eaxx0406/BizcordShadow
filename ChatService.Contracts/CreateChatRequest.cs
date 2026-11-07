namespace ChatService.Contracts
{
    // En chat oprettes med navn, opretter og de andre medlemmer i ét kald (se ADR 0012).
    // ParticipantUserIds er user-id'er (ikke participant-id'er) og skal indeholde mindst én anden bruger end opretteren.
    public record CreateChatRequest(string Name, Guid CreatedByUserId, IReadOnlyList<Guid> ParticipantUserIds);
}
