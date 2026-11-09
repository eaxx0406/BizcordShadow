using ChatService.Contracts;

namespace Bizcord.Models
{
    // Laver teksten til en systembesked (ADR 0022). ChatService sender kun HVAD der skete (SystemType + Details),
    // så teksten bestemmes her – både i chatten og i previewet i chatlisten.
    public static class SystemMessageText
    {
        public static string For(string? systemType, string? senderName, IReadOnlyDictionary<string, string>? details = null)
        {
            return systemType switch
            {
                SystemMessageTypes.MemberAdded => $"{senderName} blev tilføjet til chatten",
                SystemMessageTypes.MemberLeft => $"{senderName} forlod chatten",
                SystemMessageTypes.ChatRenamed when details?.TryGetValue("newName", out var newName) == true
                    => $"{senderName} omdøbte chatten til »{newName}«",
                SystemMessageTypes.ChatRenamed => $"{senderName} omdøbte chatten",
                // En type, frontenden ikke kender endnu (fx gruppebillede): vis noget neutralt i stedet for at fejle
                _ => $"{senderName} ændrede noget i chatten"
            };
        }
    }
}
