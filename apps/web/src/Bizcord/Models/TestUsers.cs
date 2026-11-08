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

        // Brugere, man kan logge ind som i dropdownen. Den første er standard, hvis man ikke har valgt nogen.
        // Tip: log ind som Emma og derefter som Alma – Alma kan se en chat, Emma ikke kan 🐍
        public static readonly IReadOnlyList<TestUser> CanLogIn =
        [
            User(1, "Emma"), User(2, "Oliver"), User(3, "Noah"), User(4, "Sofie"), User(5, "Lucas"),
            User(6, "Freja"), User(7, "Mikkel"), User(8, "Clara"), User(9, "Anton"),
            User(10, "Alma"), User(11, "Sofia"), User(12, "Mille"), User(13, "Laura"),
            User(14, "Victor"), User(15, "Liam"), User(16, "Oscar"), User(17, "Felix"),
            User(33, "Malthe"), User(34, "Asger"),
            User(18, "Mor"), User(19, "Far"), User(20, "Ida"), User(21, "Emil"), User(22, "Mormor"),
            User(23, "Sander"), User(24, "Peter"), User(25, "Ea"), User(26, "Bjørn"),
            User(27, "Henrik"), User(28, "Bente"), User(29, "Kasper"), User(30, "Maja"), User(31, "Rasmus"), User(32, "Pia"),
        ];

        // Alle, der kan vælges som medlem (inkl. den ukendte bruger til test af fejl)
        public static readonly IReadOnlyList<TestUser> All = [.. CanLogIn, UnknownUser];

        public static string NameOf(Guid id) => All.FirstOrDefault(u => u.Id == id)?.Name ?? id.ToString();

        // Samme id-mønster som seed'en: user_id = 10000000-0000-0000-0000-0000000000NN
        private static TestUser User(int nr, string name) =>
            new(Guid.Parse($"10000000-0000-0000-0000-{nr:D12}"), name);
    }
}
