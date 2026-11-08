using System.ComponentModel.DataAnnotations;
using ChatService.Contracts.Validation;

namespace ChatService.Contracts
{
    // SenderUserId sendes af klienten, indtil der findes login (så kommer det fra ICurrentUser).
    // [Required] afviser null, "" og tekst med kun mellemrum ("." er fint). [ApiController] svarer så automatisk 400.
    public record SendMessageRequest(
        [NotEmptyGuid] Guid SenderUserId,
        [Required, MaxLength(4000)] string Content);
}
