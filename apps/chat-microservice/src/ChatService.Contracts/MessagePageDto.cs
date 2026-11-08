namespace ChatService.Contracts
{
    // Én side beskeder i en chat, ÆLDSTE først (så de kan vises oppefra og ned).
    // HasOlder: der findes ældre beskeder. Hent dem med ?before=<id på den ældste besked i Messages>.
    public record MessagePageDto(IReadOnlyList<ChatMessageDto> Messages, bool HasOlder);

    // SenderUserId (ikke participation-id), så klienten kan se, hvilke beskeder der er ens egne (ADR 0003).
    // IsDeleted: beskeden er slettet af afsenderen. Den bliver i listen (så paging virker), men Content er tom.
    public record ChatMessageDto(Guid Id, Guid SenderUserId, string SenderName, string Content, DateTimeOffset SentAt, bool IsDeleted);
}
