using System.ComponentModel.DataAnnotations;
using ChatService.Contracts.Validation;

namespace ChatService.Contracts
{
    // En chat oprettes med navn, opretter og de andre medlemmer i ét kald (se ADR 0012).
    // ParticipantUserIds er user-id'er (ikke participant-id'er) og skal indeholde mindst én anden bruger end opretteren.
    // Attributterne giver automatisk 400 med ProblemDetails via [ApiController]. Reglen "mindst én ANDEN bruger" håndhæves i Chat.Create.
    public record CreateChatRequest(
        [Required, MaxLength(100)] string Name,
        [NotEmptyGuid] Guid CreatedByUserId,
        [Required, MinLength(1)] IReadOnlyList<Guid> ParticipantUserIds);
}
