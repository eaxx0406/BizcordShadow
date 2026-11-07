using System.Globalization;
using ChatService.Contracts;

namespace Bizcord.Models
{
    // Det, partialen _ChatMessages skal bruge: én side beskeder (ældste først), om der findes ældre,
    // og hvem "mig" er (så egne beskeder kan vises til højre).
    public record MessageList(IReadOnlyList<ChatMessageDto> Messages, bool HasOlder, Guid CurrentUserId)
    {
        private static readonly CultureInfo Danish = CultureInfo.GetCultureInfo("da-DK");

        // I dag: "10:24", ellers: "21. okt. 10:24"
        public static string FormatTime(DateTimeOffset sentAt)
        {
            var local = sentAt.ToLocalTime();
            return local.Date == DateTimeOffset.Now.Date
                ? local.ToString("HH:mm", Danish)
                : local.ToString("d. MMM HH:mm", Danish);
        }
    }
}
