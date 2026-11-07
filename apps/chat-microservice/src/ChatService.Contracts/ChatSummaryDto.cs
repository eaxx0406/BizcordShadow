namespace ChatService.Contracts
{
    // Én linje i listen "mine chats": kun det, listen skal vise (ADR 0004).
    // LastMessage* er null, indtil chatten har fået sin første besked.
    public record ChatSummaryDto(
        Guid Id,
        string Name,
        int ParticipantCount,
        string? LastMessage,
        string? LastMessageSenderName,
        DateTimeOffset? LastMessageAt);
}
