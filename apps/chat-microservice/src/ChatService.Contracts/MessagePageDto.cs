namespace ChatService.Contracts
{
    // Én side beskeder i en chat, ÆLDSTE først (så de kan vises oppefra og ned).
    // HasOlder: der findes ældre beskeder. Hent dem med ?before=<id på den ældste besked i Messages>.
    public record MessagePageDto(IReadOnlyList<ChatMessageDto> Messages, bool HasOlder);

    // SenderUserId (ikke participation-id), så klienten kan se, hvilke beskeder der er ens egne (ADR 0003).
    // IsDeleted: beskeden er slettet af afsenderen. Den bliver i listen (så paging virker), men Content er tom.
    // IsEdited: afsenderen har rettet beskeden (klienten viser "redigeret").
    // Kind: "user" eller "system" (MessageKinds). Systembeskeder (ADR 0022) har tom Content –
    // klienten laver selv teksten ud fra SystemType (SystemMessageTypes), SenderName og Details.
    public record ChatMessageDto(
        Guid Id,
        Guid SenderUserId,
        string SenderName,
        string Content,
        DateTimeOffset SentAt,
        bool IsDeleted,
        bool IsEdited,
        string Kind,
        string? SystemType,
        IReadOnlyDictionary<string, string>? Details);
}
