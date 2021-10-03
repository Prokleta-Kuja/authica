using System;
using System.Collections.Generic;
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
using Microsoft.Extensions.Caching.Memory;

namespace authica.Pages.Auth
{
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public class ResetPasswordModel : PageModel
    {
        readonly AppDbContext _db;
        readonly IPasswordHasher _hasher;
        readonly IMemoryCache _cache;
        readonly IpSecurity _ipsec;
        readonly MailService _mail;
        public readonly IResetPassword T = LocalizationFactory.ResetPassword();
        public ResetPasswordModel(AppDbContext db, IPasswordHasher hasher, IMemoryCache cache, IpSecurity ipsec, MailService mail)
        {
            _db = db;
            _hasher = hasher;
            _cache = cache;
            _ipsec = ipsec;
            _mail = mail;
        }
        public bool EmailServiceSetup => _mail.IsSetup;
        public bool EmailSent { get; set; }
        [FromRoute] public Guid? Token { get; set; }
        [FromForm] public string? Email { get; set; }
        [FromForm] public string? Password { get; set; }
        public Dictionary<string, string> Errors = new();
        string GetKey(Guid guid) => $"Reset_{guid}";
        public async Task<IActionResult> OnGet()
        {
            if (Token.HasValue && _cache.TryGetValue<int>(GetKey(Token.Value), out var userId))
            {
                var user = await _db.Users.SingleOrDefaultAsync(u => u.UserId == userId);
                if (user == null || user.Disabled.HasValue)
                    return Redirect(C.Routes.ResetPassword);

                Email = user.Email;
            }

            return Page();
        }
        public async Task<IActionResult> OnPostEmail()
        {
            if (string.IsNullOrWhiteSpace(Email))
                Errors.TryAdd(nameof(Email), T.ValidationRequired);
            else
            {
                EmailSent = true; // Must not leak that email exists or not
                var email = Email.ToLower();
                var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
                if (user != null && !user.Disabled.HasValue)
                {
                    var token = Guid.NewGuid();
                    var key = GetKey(token);
                    _cache.Set(key, user.UserId, TimeSpan.FromMinutes(15));

                    // Count reset attempt as infraction to prevent sending a ton of email
                    if (_ipsec.LogResetPassword())
                        return StatusCode(StatusCodes.Status418ImATeapot, T.IpBlocked);

                    if (EmailServiceSetup)
                        await _mail.SendPasswordResetAsync(user, token);
                    System.Diagnostics.Debug.WriteLine($"Reset link {C.Routes.ResetPassword}/{token} for user {user.UserName}");
                }
            }

            return Page();
        }
        public async Task<IActionResult> OnPost()
        {
            if (string.IsNullOrWhiteSpace(Password)
                || !Token.HasValue
                || !_cache.TryGetValue<int>(GetKey(Token.Value), out var userId))
                return Page();

            var user = await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.Disabled.HasValue)
                return Redirect(C.Routes.ResetPassword);

            user.SetPassword(Password, _hasher);
            user.EmailVerified = true;
            user.LastLogin = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _cache.Remove(GetKey(Token.Value));

            var principal = CookieAuth.CreatePrincipal(user);
            var props = CookieAuth.CreateAuthProps();

            await HttpContext.SignInAsync(CookieAuth.Scheme, principal, props);
            return Redirect(C.Routes.Root); // TODO: redirect to user info page
        }
    }
}