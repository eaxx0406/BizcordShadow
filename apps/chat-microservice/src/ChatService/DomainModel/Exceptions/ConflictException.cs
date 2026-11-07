namespace ChatService.DomainModel.Exceptions
{
    // Kastes, når en handling bryder en domæneregel (f.eks. allerede medlem, eller chatten er fuld).
    // Den globale exception handler oversætter den til 409 (ADR 0011).
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}
