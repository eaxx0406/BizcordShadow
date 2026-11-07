using Bizcord.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bizcord.Pages
{
    // Velkomst-dashboard (wireframe). Testformularerne for chat ligger nu på Chat-siden.
    public class IndexModel : PageModel
    {
        private readonly CurrentUser _currentUser;

        public IndexModel(CurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public string UserName => _currentUser.Name;

        public void OnGet()
        {
        }
    }
}
