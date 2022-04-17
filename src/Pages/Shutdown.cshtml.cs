using authica.Auth;
using authica.Translations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace authica.Pages;

public class ShutdownModel : PageModel
{
    public IShutdown T = LocalizationFactory.Shutdown();
    public CurrentSession Session;
    public bool IsRestart;

    public ShutdownModel(CurrentSession session)
    {
        Session = session;
    }

    [FromRoute] public string? Slug { get; set; }
    public IActionResult OnGet()
    {
        IsRestart = !string.IsNullOrWhiteSpace(Slug);
        return Page();
    }
}