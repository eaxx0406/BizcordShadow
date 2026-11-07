using Bizcord.Models;
using Bizcord.Services;
using ChatService.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bizcord.Pages
{
    public class IndexModel : PageModel
    {
        private readonly GatewayClient _gateway;

        public IndexModel(GatewayClient gateway)
        {
            _gateway = gateway;
        }

        public string UserName => CurrentUser.Name;
        public Guid UserId => CurrentUser.Id;

        public string? StatusMessage { get; private set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostCreateChat()
        {
            try
            {
                var ok = await _gateway.CreateChatAsync(new CreateChatRequest("Ny chat", CurrentUser.Id));
                StatusMessage = ok ? "Chat oprettet." : "Gatewayen svarede med en fejl.";
            }
            catch (HttpRequestException)
            {
                StatusMessage = "Kunne ikke få forbindelse til GatewayAPI. Kører den?";
            }
            return Page();
        }
    }
}
