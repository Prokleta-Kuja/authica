using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using authica.Entities;
using authica.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
        public SignInModel(AppDbContext db, IPasswordHasher hasher)
        {
            _db = db;
            _hasher = hasher;
        }
        public IActionResult OnGet() => Page();
        [FromForm] public string? Username { get; set; }
        [FromForm] public string? Password { get; set; }
        public Dictionary<string, string> Errors = new();
        public async Task<IActionResult> OnPost()
        {
            Errors.Clear();

            if (string.IsNullOrWhiteSpace(Username))
                Errors.TryAdd(nameof(Username), "Required");

            if (string.IsNullOrWhiteSpace(Password))
                Errors.TryAdd(nameof(Password), "Required");

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
                Errors.TryAdd(string.Empty, "Invalid username/email and/or password");
                //TODO: log error and ip
                return Page();
            }

            var redirectUri = C.Routes.Root;
            // TODO: Validate app and change redirectUri

            var authenticated = user.VerifyPassword(password, _hasher);
            if (authenticated)
            {
                user.LastLogin = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                var identity = CreateIdentity(user);

                var props = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IssuedUtc = DateTime.UtcNow,
                    RedirectUri = redirectUri,
                };

                return SignIn(new ClaimsPrincipal(identity), props, CookieAuthenticationDefaults.AuthenticationScheme);
            }
            else
            {
                ModelState.AddModelError(nameof(Username), "Invalid username/email and/or password");
                //TODO: log error and ip
                return Page();
            }
        }
        ClaimsIdentity CreateIdentity(User user)
        {
            var claims = new List<Claim>
                {
                    new(C.Claims.Subject, user.AliasId.ToString()),
                    new(C.Claims.UserName, user.UserName),
                    new(C.Claims.Email, user.Email),
                    new(C.Claims.EmailVerified, user.EmailVerified.ToString(), ClaimValueTypes.Boolean),
                };

            var hasFirstName = !string.IsNullOrWhiteSpace(user.FirstName);
            var hasLastName = !string.IsNullOrWhiteSpace(user.LastName);

            if (hasFirstName)
                claims.Add(new(C.Claims.FirstName, user.FirstName!));

            if (hasLastName)
                claims.Add(new(C.Claims.LastName, user.LastName!));

            if (hasFirstName && hasLastName)
                claims.Add(new(C.Claims.DisplayName, $"{user.FirstName} {user.LastName}"));

            if (!string.IsNullOrWhiteSpace(user.TimeZone))
                claims.Add(new(C.Claims.TimeZone, user.TimeZone));

            if (!string.IsNullOrWhiteSpace(user.Locale))
                claims.Add(new(C.Claims.Locale, user.Locale));

            if (user.UserRoles!.Any())
                claims.AddRange(user.UserRoles!.Select(ur => new Claim(ClaimTypes.Role, ur.Role!.Name)));

            var identity = new ClaimsIdentity(claims, "Basic");
            return identity;
        }
    }
}