using ChatService.Contracts.Validation;

namespace ChatService.Contracts
{
    public record AddParticipantRequest([NotEmptyGuid] Guid UserId);
}
