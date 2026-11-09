using System.ComponentModel.DataAnnotations;
using ChatService.Contracts.Validation;

namespace ChatService.Contracts
{
    // Afsenderen retter sin besked (inden for 15 minutter, ADR 0020). Samme regler for teksten som SendMessageRequest.
    // UserId sendes af klienten, indtil der findes login.
    public record EditMessageRequest(
        [NotEmptyGuid] Guid UserId,
        [Required, MaxLength(4000)] string Content);
}
