using System.ComponentModel.DataAnnotations;

namespace ChatService.Contracts.Validation
{
    // [Required] afviser ikke Guid.Empty (00000000-...), fordi en Guid aldrig er null.
    // Denne attribut gør, så [ApiController] automatisk svarer 400 med ProblemDetails, hvis et id er tomt.
    // Kan bruges både på felter i kontrakterne og direkte på parametre i controllere (f.eks. ?userId=).
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.Field)]
    public class NotEmptyGuidAttribute : ValidationAttribute
    {
        public NotEmptyGuidAttribute() : base("{0} må ikke være tom.")
        {
        }

        public override bool IsValid(object? value) => value is Guid id && id != Guid.Empty;
    }
}
