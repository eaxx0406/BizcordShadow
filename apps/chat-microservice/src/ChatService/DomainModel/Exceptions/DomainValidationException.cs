namespace ChatService.DomainModel.Exceptions
{
    // Kastes, når en værdi er ugyldig efter domænets regler (f.eks. et tomt chatnavn,
    // eller en chat uden andre medlemmer end opretteren).
    // Den globale exception handler oversætter den til 400 (ADR 0011).
    public class DomainValidationException : Exception
    {
        public DomainValidationException(string message) : base(message)
        {
        }
    }
}
