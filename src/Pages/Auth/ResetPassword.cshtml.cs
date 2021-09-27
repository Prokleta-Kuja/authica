using System;
using System.Collections.Generic;
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
    public class ResetPasswordModel : PageModel
    {
        readonly AppDbContext _db;
        readonly IPasswordHasher _hasher;
        public ResetPasswordModel(AppDbContext db, IPasswordHasher hasher)
        {
            _db = db;
            _hasher = hasher;
        }
        public bool EmailSent { get; set; }
        [FromRoute] public Guid? Token { get; set; }
        [FromForm] public string? Email { get; set; }
        [FromForm] public string? Password { get; set; }
        public Dictionary<string, string> Errors = new();
        public async Task<IActionResult> OnGet()
        {
            if (Token.HasValue)
            {
                var user = await _db.Users.SingleOrDefaultAsync(u => u.ResetToken == Token);
                if (user == null || user.Disabled.HasValue)
                    return Redirect(C.Routes.ResetPassword);

                Email = user.Email;
            }

            return Page();
        }
        public async Task<IActionResult> OnPostEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
                Errors.TryAdd(nameof(Email), "Required");
            else
            {
                EmailSent = true; // Must not leak that email exists or not
                var email = Email.ToLower();
                var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
                if (user != null && !user.Disabled.HasValue)
                {
                    user.ResetToken = Guid.NewGuid();
                    await _db.SaveChangesAsync();
                    // TODO: send token via email
                    System.Diagnostics.Debug.WriteLine($"Reset link {C.Routes.ResetPassword}/{user.ResetToken}");
                }
            }

            return Page();
        }
        public async Task<IActionResult> OnPost()
        {
            if (string.IsNullOrWhiteSpace(Password))
                return Page();

            var user = await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.ResetToken == Token);

            if (user == null || user.Disabled.HasValue)
                return Redirect(C.Routes.ResetPassword);

            user.SetPassword(Password, _hasher);
            user.EmailVerified = true;
            user.ResetToken = null;
            user.LastLogin = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var principal = CookieAuth.CreatePrincipal(user);
            var props = CookieAuth.CreateAuthProps();

            await HttpContext.SignInAsync(CookieAuth.Scheme, principal, props);
            return Redirect(C.Routes.Root); // TODO: redirect to user info page
        }
    }
}