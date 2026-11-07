using System.Net.Http.Json;
using ChatService.Contracts;

namespace Bizcord.Services
{
    // Typed HttpClient mod GatewayAPI. BaseAddress kommer fra appsettings ("GatewayApi:BaseUrl").
    // Request-typer kommer fra ChatService.Contracts (se ADR 0007), så frontend og ChatService altid er enige om formatet.
    public class GatewayClient
    {
        private readonly HttpClient _http;

        public GatewayClient(HttpClient http)
        {
            _http = http;
        }

        // POST api/chats -> GatewayAPI (Ocelot) -> ChatService
        public async Task<bool> CreateChatAsync(CreateChatRequest request, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/chats", request, ct);
            return response.IsSuccessStatusCode;
        }
    }
}
