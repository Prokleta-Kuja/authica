using authica.Auth;
using authica.Translations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace authica.Pages.Auth;

[AllowAnonymous]
public class SignOutModel : PageModel
{
    public ISignOut T = LocalizationFactory.SignOut();
    public CurrentSession Session;

    public SignOutModel(CurrentSession session)
    {
        Session = session;
    }

    [FromRoute] public string? Slug { get; set; }
    public IActionResult OnGet()
    {
        T = string.IsNullOrWhiteSpace(Slug) ? LocalizationFactory.SignOut(Session.LocaleId) : LocalizationFactory.SignOut(Slug);

        if (!Session.IsAuthenticated)
            return Page();

        var props = new AuthenticationProperties { RedirectUri = $"{C.Routes.SignOut}/{Session.LocaleId}" };
        return SignOut(props, CookieAuthenticationDefaults.AuthenticationScheme);
    }
}