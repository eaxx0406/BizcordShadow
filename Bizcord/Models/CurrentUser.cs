namespace Bizcord.Models
{
    // "Dev login": den bruger, man er logget ind som, vælges i dropdownen i topbaren og gemmes i en cookie.
    // Erstatter et rigtigt login, fordi UserService er uden for scope. I virkeligheden ville user-id'et
    // komme fra et login-token (claims) fra en identity provider, og resten af frontenden ville være uændret.
    public class CurrentUser
    {
        public const string CookieName = "bizcord-user";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Brugeren fra cookien, eller den første testbruger, hvis cookien mangler eller er ugyldig
        public TestUser User
        {
            get
            {
                var cookie = _httpContextAccessor.HttpContext?.Request.Cookies[CookieName];
                return Guid.TryParse(cookie, out var id)
                    ? TestUsers.CanLogIn.FirstOrDefault(u => u.Id == id) ?? TestUsers.CanLogIn[0]
                    : TestUsers.CanLogIn[0];
            }
        }

        public Guid Id => User.Id;
        public string Name => User.Name;
    }
}
