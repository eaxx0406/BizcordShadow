namespace Bizcord.Models
{
    // Én linje i chatlisten: navn, seneste besked, tidspunkt og antal ulæste.
    public record ChatPreview(string Name, string LastMessage, string Time, int Unread, bool IsGroup);

    // Dummydata til wireframet, indtil ChatService har et endpoint, der returnerer brugerens chats
    // med seneste besked og ulæst-tal. Skal erstattes af et kald gennem GatewayClient.
    public static class DummyChats
    {
        public static readonly IReadOnlyList<ChatPreview> All =
        [
            new("Projektgruppen", "Peter: Lyder godt! 👍", "10:24", 1, IsGroup: true),
            new("Sander", "Sender lige filen.", "I går", 0, IsGroup: false),
            new("Systemdesign", "Ea: Har opdateret diagrammet.", "I går", 0, IsGroup: true),
            new("Peter", "Super, tak!", "21. okt.", 0, IsGroup: false),
        ];
    }
}
