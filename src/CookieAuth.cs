using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace authica
{
    public static class CookieAuth
    {
        public static Action<CookieAuthenticationOptions> Configure = o =>
        {
            o.Cookie.Name = $".{C.Configuration.Current.Name}";
            o.Cookie.IsEssential = true;
            o.Cookie.HttpOnly = true;
            o.Cookie.Path = C.Routes.Root;
            o.Cookie.Domain = C.Configuration.Current.Domain;
            o.Cookie.SameSite = SameSiteMode.None;
            o.Cookie.MaxAge = C.Configuration.Current.MaxSessionDuration;

            o.SessionStore = new MemoryCacheTicketStore();
            o.ClaimsIssuer = C.Configuration.Current.Name;
            o.LoginPath = C.Routes.SignIn;
            o.LogoutPath = C.Routes.SignOut;
            o.AccessDeniedPath = C.Routes.Forbidden;
            o.ExpireTimeSpan = C.Configuration.Current.MaxSessionDuration;
        };
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