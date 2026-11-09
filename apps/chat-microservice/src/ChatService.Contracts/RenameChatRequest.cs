using System.ComponentModel.DataAnnotations;
using ChatService.Contracts.Validation;

namespace ChatService.Contracts
{
    // PUT api/chats/{chatId}: giver chatten et nyt navn. Navnet er det eneste, der kan ændres på en chat.
    // UserId er den, der omdøber (sendes af klienten, indtil der findes login). Kun aktive medlemmer må omdøbe.
    public record RenameChatRequest(
        [NotEmptyGuid] Guid UserId,
        [Required, MaxLength(100)] string Name);
}
