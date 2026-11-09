using System.Text.Json;

namespace ChatService.Repositories
{
    // details-kolonnen (jsonb, ADR 0022) læses som tekst og laves om til en ordbog her,
    // så begge besked-queries gør det på samme måde.
    public static class DetailsJson
    {
        public static IReadOnlyDictionary<string, string>? Read(string? json)
        {
            return json is null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
    }
}
