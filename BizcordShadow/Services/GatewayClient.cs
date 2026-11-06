using System.Net.Http.Json;

namespace BizcordShadow.Services
{
    public record CreateChatRequest(string Name, Guid CreatedByUserId);

    // Typed HttpClient mod GatewayAPI. BaseAddress kommer fra appsettings ("GatewayApi:BaseUrl").
    public class GatewayClient
    {
        private readonly HttpClient _http;

        public GatewayClient(HttpClient http)
        {
            _http = http;
        }

        // TODO: Tilpas route/payload når GatewayAPI har et create-chat endpoint.
        public async Task<bool> CreateChatAsync(CreateChatRequest request, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/chats", request, ct);
            return response.IsSuccessStatusCode;
        }
    }
}
