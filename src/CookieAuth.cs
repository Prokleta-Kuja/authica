using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using authica.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace authica
{
    public static class CookieAuth
    {
        public const string Scheme = CookieAuthenticationDefaults.AuthenticationScheme;
        public static Action<CookieAuthenticationOptions> Configure = o =>
        {
            o.Cookie.Name = $".{C.Configuration.Current.Name}";
            o.Cookie.IsEssential = true;
            o.Cookie.HttpOnly = true;
            o.Cookie.Path = C.Routes.Root;
            o.Cookie.Domain = $".{C.Configuration.Current.Domain}";
            o.Cookie.SameSite = SameSiteMode.None;
            o.Cookie.MaxAge = C.Configuration.Current.MaxSessionDuration;

            o.SessionStore = new MemoryCacheTicketStore();
            o.ClaimsIssuer = C.Configuration.Current.Name;
            o.LoginPath = C.Routes.SignIn;
            o.LogoutPath = C.Routes.SignOut;
            o.AccessDeniedPath = C.Routes.Forbidden;
            o.ExpireTimeSpan = C.Configuration.Current.MaxSessionDuration;
        };
        public static AuthenticationProperties CreateAuthProps() =>
            new AuthenticationProperties
            {
                AllowRefresh = true,
                IssuedUtc = DateTime.UtcNow,
            };
        public static ClaimsPrincipal CreatePrincipal(User user) => new ClaimsPrincipal(CreateIdentity(user));
        public static ClaimsIdentity CreateIdentity(User user)
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
        public class MemoryCacheTicketStore : ITicketStore
        {
            private const string KeyPrefix = "AuthSessionStore-";
            private IMemoryCache _cache;

            public MemoryCacheTicketStore()
            {
                _cache = new MemoryCache(new MemoryCacheOptions());
            }

            public async Task<string> StoreAsync(AuthenticationTicket ticket)
            {
                var guid = Guid.NewGuid();
                var key = KeyPrefix + guid.ToString();
                await RenewAsync(key, ticket);

                return key;
            }

            public Task RenewAsync(string key, AuthenticationTicket ticket)
            {
                var options = new MemoryCacheEntryOptions();
                var expiresUtc = ticket.Properties.ExpiresUtc;
                if (expiresUtc.HasValue)
                {
                    options.SetAbsoluteExpiration(expiresUtc.Value);
                }

                _cache.Set(key, ticket, options);

                return Task.CompletedTask;
            }

            public Task<AuthenticationTicket> RetrieveAsync(string key)
            {
                AuthenticationTicket ticket;
                _cache.TryGetValue(key, out ticket);
                return Task.FromResult(ticket);
            }

            public Task RemoveAsync(string key)
            {
                _cache.Remove(key);
                return Task.CompletedTask;
            }
        }
    }
}