namespace Bizcord.Models
{
    public record TestUser(Guid Id, string Name);

    // Hardcodede testbrugere, fordi UserService er uden for scope.
    // Skal matche participants i apps/chat-microservice/database/02-seed.sql (som simulerer de events, UserService ville sende).
    // "Ukendt bruger" findes med vilje IKKE i databasen, så fejlhåndteringen (404) kan testes.
    public static class TestUsers
    {
        public static readonly TestUser UnknownUser =
            new(Guid.Parse("99999999-9999-9999-9999-999999999999"), "Ukendt bruger (test af fejl)");

        // Brugere, man kan logge ind som i dropdownen
        public static readonly IReadOnlyList<TestUser> CanLogIn =
        [
            new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Test Bruger"),
            new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Anna"),
            new(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Bo"),
            new(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Carla"),
        ];

        // Alle, der kan vælges som medlem (inkl. den ukendte bruger til test af fejl)
        public static readonly IReadOnlyList<TestUser> All = [.. CanLogIn, UnknownUser];

        public static string NameOf(Guid id) => All.FirstOrDefault(u => u.Id == id)?.Name ?? id.ToString();
    }
}
