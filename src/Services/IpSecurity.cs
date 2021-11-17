using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using authica.Translations;
using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace authica.Services
{
    public static class IpSecurityExtensions
    {
        public static void UseIpSecurity(this IApplicationBuilder app) => app.UseMiddleware<IpSecurityMiddleware>();
    }
    public class IpSecurityMiddleware
    {
        readonly RequestDelegate _next;
        readonly IIpSecurity _t = LocalizationFactory.IpSecurity();

        public IpSecurityMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext httpContext, IpSecurity ipsec)
        {
            if (ipsec.IsAllowed())
                await _next(httpContext);
            else
            {
                httpContext.Response.StatusCode = StatusCodes.Status418ImATeapot;
                await httpContext.Response.WriteAsync(_t.IpBlocked);
            }
        }
    }
    public class IpSecurity
    {
        readonly ILogger<IpSecurity> _logger;
        readonly IMemoryCache _cache;
        readonly string ipAddress;
        public IpSecurity(ILogger<IpSecurity> logger,
                          IMemoryCache memoryCache,
                          IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _cache = memoryCache;
            ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        }
        string BanKey => $"Banned_{ipAddress}";
        string SignInKey => $"SignIn_{ipAddress}";
        string ResetPasswordKey => $"Reset_{ipAddress}";
        bool LogInfraction(string key)
        {
            _cache.TryGetValue<int>(key, out var prevInfractions);

            var newInfractions = prevInfractions + 1;
            if (newInfractions >= C.Configuration.Current.MaxInfractions)
            {
                _cache.Remove(key);
                Ban();
                return true;
            }

            _cache.Set<int>(key, newInfractions, C.Configuration.Current.InfractionExpiration);
            return false;
        }
        public void Ban() => _cache.Set<bool>(BanKey, true, C.Configuration.Current.BanTime);
        public void UnBan() => _cache.Remove(BanKey);
        public bool LogSignIn() => LogInfraction(SignInKey);
        public bool LogResetPassword() => LogInfraction(ResetPasswordKey);
        public bool IsAllowed()
        {
            if (_cache.TryGetValue(BanKey, out bool exists))
                return false;

            // Perform country check only if configured
            if (C.Configuration.Current.AllowedCountryCodes.Any())
            {
                // Do not download db on signin/reset request
                if (!C.GeoLocationDbFile.Exists)
                    return true;

                using var reader = new DatabaseReader(C.GeoLocationDbFile.FullName);
                if (reader.TryCountry(ipAddress, out var response))
                {
                    var allowed = C.Configuration.Current.AllowedCountryCodes.Contains(response?.Country?.IsoCode ?? string.Empty);
                    if (!allowed)
                    {
                        Ban(); // To speed up further lookups
                        return false;
                    }
                }
            }

            return true;
        }
    }
}