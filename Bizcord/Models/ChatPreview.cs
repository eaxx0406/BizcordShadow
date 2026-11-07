using System.Globalization;
using ChatService.Contracts;

namespace Bizcord.Models
{
    // Én linje i chatlisten: navn, seneste besked, tidspunkt og antal ulæste.
    // Unread er altid 0, indtil ChatService kan tælle ulæste beskeder (message_receipt).
    public record ChatPreview(Guid Id, string Name, string LastMessage, string Time, int Unread, bool IsGroup)
    {
        private static readonly CultureInfo Danish = CultureInfo.GetCultureInfo("da-DK");

        // Oversætter et svar fra ChatService (GET api/chats?userId=...) til det, listen viser
        public static ChatPreview From(ChatSummaryDto chat)
        {
            var lastMessage = chat.LastMessage is null
                ? "Ingen beskeder endnu"
                : $"{chat.LastMessageSenderName}: {chat.LastMessage}";

            return new ChatPreview(
                chat.Id,
                chat.Name,
                lastMessage,
                FormatTime(chat.LastMessageAt),
                Unread: 0,
                IsGroup: chat.ParticipantCount > 2);
        }

        // I dag: "10:24", i går: "I går", ellers: "21. okt."
        private static string FormatTime(DateTimeOffset? sentAt)
        {
            if (sentAt is null)
            {
                return string.Empty;
            }

            var local = sentAt.Value.ToLocalTime();
            var today = DateTimeOffset.Now.Date;

            if (local.Date == today) return local.ToString("HH:mm", Danish);
            if (local.Date == today.AddDays(-1)) return "I går";
            return local.ToString("d. MMM", Danish);
        }
    }
}
