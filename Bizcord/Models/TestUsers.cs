namespace Bizcord.Models
{
    public record TestUser(Guid Id, string Name);

    // Hardcodede testbrugere indtil der findes en UserService at slå brugere op i.
    // Skal matche participants i database/chatservice/02-seed.sql.
    // "Ukendt bruger" findes med vilje IKKE i databasen, så fejlhåndteringen (404) kan testes.
    public static class TestUsers
    {
        public static readonly IReadOnlyList<TestUser> All =
        [
            new(CurrentUser.Id, CurrentUser.Name),
            new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Anna"),
            new(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Bo"),
            new(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Carla"),
            new(Guid.Parse("99999999-9999-9999-9999-999999999999"), "Ukendt bruger (test af fejl)"),
        ];

        public static string NameOf(Guid id) => All.FirstOrDefault(u => u.Id == id)?.Name ?? id.ToString();
    }
}
