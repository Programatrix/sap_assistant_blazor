using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SAPAssistant.Areas.Account.Pages
{
    public class LogoutModel : PageModel
    {
        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            await HttpContext.SignOutAsync();
            return LocalRedirect("/Account/Login");
        }
    }
}
