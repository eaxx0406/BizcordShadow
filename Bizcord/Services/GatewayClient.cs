using System.Net.Http.Json;
using System.Text.Json;
using ChatService.Contracts;
using Microsoft.AspNetCore.Http;

namespace Bizcord.Services
{
    // Typed HttpClient mod GatewayAPI. BaseAddress kommer fra appsettings ("GatewayApi:BaseUrl").
    // Request-typer kommer fra ChatService.Contracts (se ADR 0007), så frontend og ChatService altid er enige om formatet.
    // Fejl fra services kommer som ProblemDetails (ADR 0011) og oversættes til en læsbar besked.
    public class GatewayClient
    {
        private const string ConnectionError = "Kunne ikke få forbindelse til GatewayAPI. Kører den?";

        private readonly HttpClient _http;

        public GatewayClient(HttpClient http)
        {
            _http = http;
        }

        // POST api/chats -> GatewayAPI (Ocelot) -> ChatService
        public async Task<GatewayResult<ChatDto>> CreateChatAsync(CreateChatRequest request, CancellationToken ct = default)
        {
            try
            {
                using var response = await _http.PostAsJsonAsync("api/chats", request, ct);
                if (!response.IsSuccessStatusCode)
                {
                    return GatewayResult<ChatDto>.Fail(await ReadErrorAsync(response, ct));
                }

                var chat = await response.Content.ReadFromJsonAsync<ChatDto>(ct);
                return chat is null
                    ? GatewayResult<ChatDto>.Fail("ChatService svarede uden en chat.")
                    : GatewayResult<ChatDto>.Ok(chat);
            }
            catch (HttpRequestException)
            {
                return GatewayResult<ChatDto>.Fail(ConnectionError);
            }
        }

        // POST api/chats/{chatId}/participants -> GatewayAPI (Ocelot) -> ChatService (ADR 0010)
        public async Task<GatewayResult> AddParticipantAsync(Guid chatId, Guid userId, CancellationToken ct = default)
        {
            try
            {
                using var response = await _http.PostAsJsonAsync($"api/chats/{chatId}/participants", new AddParticipantRequest(userId), ct);
                return response.IsSuccessStatusCode
                    ? GatewayResult.Ok()
                    : GatewayResult.Fail(await ReadErrorAsync(response, ct));
            }
            catch (HttpRequestException)
            {
                return GatewayResult.Fail(ConnectionError);
            }
        }

        // Laver en fejlbesked ud fra svaret: statuskode + title/detail/valideringsfejl fra ProblemDetails, hvis de findes.
        private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
        {
            var status = $"{(int)response.StatusCode} {response.ReasonPhrase}";

            // Ocelot svarer f.eks. 404 uden body, hvis der ikke findes en route.
            if (response.Content.Headers.ContentLength == 0)
            {
                return $"{status} (intet svar – mangler der en route i Ocelot.json?)";
            }

            try
            {
                // HttpValidationProblemDetails = ProblemDetails + "errors" (bruges ved valideringsfejl, 400)
                var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(ct);
                if (problem is null)
                {
                    return status;
                }

                var parts = new List<string>();
                if (!string.IsNullOrWhiteSpace(problem.Title)) parts.Add(problem.Title);
                if (!string.IsNullOrWhiteSpace(problem.Detail)) parts.Add(problem.Detail);
                parts.AddRange(problem.Errors.SelectMany(e => e.Value.Select(msg => $"{e.Key}: {msg}")));

                return parts.Count == 0 ? status : $"{status}: {string.Join(" – ", parts)}";
            }
            catch (Exception ex) when (ex is JsonException or NotSupportedException)
            {
                // Svaret var ikke ProblemDetails (f.eks. tekst eller HTML) – vis bare statuskoden.
                return status;
            }
        }
    }
}
