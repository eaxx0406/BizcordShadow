using BizcordShadow.Models;
using BizcordShadow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BizcordShadow.Pages
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

        public IActionResult OnPostCreateChat()
        {
            // TODO: Aktivér når GatewayAPI er klar:
            // var ok = await _gateway.CreateChatAsync(new CreateChatRequest("Ny chat", CurrentUser.Id));
            // (gør metoden async Task<IActionResult>)
            StatusMessage = "Create chat er ikke koblet til GatewayAPI endnu.";
            return Page();
        }
    }
}
