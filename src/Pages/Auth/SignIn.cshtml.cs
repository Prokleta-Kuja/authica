using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using authica.Entities;
using authica.Services;
using Microsoft.AspNetCore.Authentication;
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

                var principal = CookieAuth.CreatePrincipal(user);
                var props = CookieAuth.CreateAuthProps();

                await HttpContext.SignInAsync(CookieAuth.Scheme, principal, props);
                return Redirect(redirectUri);
            }
            else
            {
                ModelState.AddModelError(nameof(Username), "Invalid username/email and/or password");
                //TODO: log error and ip
                return Page();
            }
        }

    }
}