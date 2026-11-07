namespace ChatService.DomainModel.Exceptions
{
    // Kastes, når noget, der bliver spurgt efter, ikke findes (f.eks. en chat eller en bruger).
    // Typen beskriver domænet, ikke HTTP: den globale exception handler oversætter den til 404 (ADR 0011).
    public class NotFoundException : Exception
    {
        // Beskeden sendes videre til Exception, så den kan læses i exception.Message
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
