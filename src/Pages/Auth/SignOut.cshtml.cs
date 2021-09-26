using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace authica.Pages.Auth
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class SignOutModel : PageModel
    {
        [FromRoute] public string? Slug { get; set; }
        public IActionResult OnGet()
        {
            if (!string.IsNullOrWhiteSpace(Slug))
                return Page();

            var props = new AuthenticationProperties { RedirectUri = $"{C.Routes.SignOut}/complete" };
            return SignOut(props, CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}