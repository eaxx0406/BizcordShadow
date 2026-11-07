namespace ChatService.DomainModel.Exceptions
{
    public class ForbiddenException:Exception

    {
        public ForbiddenException(string message): base(message)
        {
            
        }
    }
}
