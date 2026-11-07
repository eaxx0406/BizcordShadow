namespace ChatService.Contracts
{
    // Én chat med dens aktive medlemmer, til visning af selve chatten.
    // Medlemmerne er med, så klienten kan vise navne (f.eks. på afsenderen af en besked).
    public record ChatDetailsDto(Guid Id, string Name, IReadOnlyList<ChatMemberDto> Participants);

    // UserId (ikke participant-id), fordi det er det id, klienten kender (se ADR 0003).
    public record ChatMemberDto(Guid UserId, string Name);
}
