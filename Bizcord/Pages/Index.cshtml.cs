using Bizcord.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bizcord.Pages
{
    // Velkomst-dashboard (wireframe). Testformularerne for chat ligger nu på Chat-siden.
    public class IndexModel : PageModel
    {
        public string UserName => CurrentUser.Name;

        public void OnGet()
        {
        }
    }
}
