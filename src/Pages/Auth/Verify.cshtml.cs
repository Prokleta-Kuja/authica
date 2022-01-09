using System;
using System.Threading.Tasks;
using authica.Auth;
using authica.Translations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace authica.Pages.Auth
{
    [IgnoreAntiforgeryToken]
    public class VerifyModel : PageModel
    {
        public IVerify T = LocalizationFactory.Verify();
        public CurrentSession Session;
        public AuthorizationStore AuthzStore;

        public VerifyModel(CurrentSession session, AuthorizationStore authzStore)
        {
            Session = session;
            AuthzStore = authzStore;
        }

        [FromQuery] public string? Rd { get; set; }
        public bool Invalid { get; set; } = true;
        public bool HasApp { get; set; }
        public bool Authorized { get; set; }
        public async Task<IActionResult> OnGet()
        {
            if (string.IsNullOrWhiteSpace(Rd) || !Uri.TryCreate(Rd, UriKind.Absolute, out var redirectTo))
                return Page();

            Invalid = false;

            HasApp = await AuthzStore.HasApp(redirectTo);
            if (!HasApp)
                return Page();

            Authorized = await AuthzStore.IsAuthorizedAsync(redirectTo, Session.UserAliasId);

            if (!Authorized)
                return Page();

            return Redirect(Rd);
        }
    }
}