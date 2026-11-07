namespace Bizcord.Models
{
    // Hardcoded bruger indtil der findes login/UserService.
    public static class CurrentUser
    {
        public static readonly Guid Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public const string Name = "Test Bruger";
    }
}
