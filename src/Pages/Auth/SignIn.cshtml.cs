using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using authica.Entities;
using authica.Services;
using authica.Translations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace authica.Pages.Auth
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class SignInModel : PageModel
    {
        readonly AppDbContext _db;
        readonly IPasswordHasher _hasher;
        readonly IpSecurity _ipsec;
        public readonly ISignIn T = LocalizationFactory.SignIn();
        public SignInModel(AppDbContext db, IPasswordHasher hasher, IpSecurity ipsec)
        {
            _db = db;
            _hasher = hasher;
            _ipsec = ipsec;
        }
        public IActionResult OnGet() => Page();
        [FromForm] public string? Username { get; set; }
        [FromForm] public string? Password { get; set; }
        public Dictionary<string, string> Errors = new();
        public async Task<IActionResult> OnPost()
        {
            Errors.Clear();

            if (string.IsNullOrWhiteSpace(Username))
                Errors.TryAdd(nameof(Username), T.ValidationRequired);

            if (string.IsNullOrWhiteSpace(Password))
                Errors.TryAdd(nameof(Password), T.ValidationRequired);

            if (Errors.Any())
                return Page();

            var username = Username!.ToLower().Trim();
            var password = Password!.Trim();

            var user = await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.UserName == username || u.Email == username);

            if (user == null || user.Disabled.HasValue)
            {
                if (_ipsec.LogSignIn())
                    return StatusCode(StatusCodes.Status418ImATeapot, T.IpBlocked);

                Errors.TryAdd(string.Empty, T.ValidationInvalid);
                return Page();
            }

            var redirectUri = C.Routes.Root;
            // TODO: Validate app and change redirectUri

            var authenticated = user.VerifyPassword(password, _hasher);
            if (authenticated)
            {
                user.LastLogin = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                var principal = CookieAuth.CreatePrincipal(user);
                var props = CookieAuth.CreateAuthProps();

                await HttpContext.SignInAsync(CookieAuth.Scheme, principal, props);
                return Redirect(redirectUri);
            }
            else
            {
                if (_ipsec.LogSignIn())
                    return StatusCode(StatusCodes.Status418ImATeapot, T.IpBlocked);

                Errors.TryAdd(string.Empty, T.ValidationInvalid);
                return Page();
            }
        }

    }
}