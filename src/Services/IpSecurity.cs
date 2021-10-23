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
        public const string DbFileName = "GeoLite2-Country.mmdb";
        public readonly FileInfo DbFile;
        readonly ILogger<IpSecurity> _logger;
        readonly IHttpClientFactory _httpClientFactory;
        readonly IMemoryCache _cache;
        readonly string ipAddress;
        public IpSecurity(ILogger<IpSecurity> logger,
                          IHttpClientFactory httpClientFactory,
                          IMemoryCache memoryCache,
                          IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _cache = memoryCache;
            DbFile = new(C.Paths.AppDataFor(DbFileName));
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
                if (!DbFile.Exists)
                    return true;

                using var reader = new DatabaseReader(DbFile.FullName);
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
        public async ValueTask<bool> DownloadDb(string? licenseKeyOverride = null)
        {
            try
            {
                _logger.LogInformation("Downloading MaxMind db");

                var licenceKey = licenseKeyOverride == null
                    ? C.Configuration.Current.MaxMindLicenseKey
                    : licenseKeyOverride;

                if (string.IsNullOrWhiteSpace(licenceKey))
                {
                    _logger.LogError("Can not download MaxMind geolocation database without license key");
                    return false;
                }

                using var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://download.maxmind.com/app/geoip_download?edition_id=GeoLite2-Country&license_key={licenceKey}&suffix=tar.gz");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Could not download MaxMind geolocation database");
                    return false;
                }

                using var input = response.Content.ReadAsStream();
                using var gzip = new GZipStream(input, CompressionMode.Decompress);
                using var decopmressed = new MemoryStream();

                await gzip.CopyToAsync(decopmressed);
                decopmressed.Seek(0, SeekOrigin.Begin);

                // Process tar file
                var buffer = new byte[100];
                while (true)
                {
                    decopmressed.Read(buffer, 0, 100);
                    var name = Encoding.ASCII.GetString(buffer).Trim('\0');

                    if (string.IsNullOrWhiteSpace(name)) // End of file
                        break;

                    if (Path.GetFileName(name) != Path.GetFileName(DbFile.FullName))
                    {
                        decopmressed.Seek(24, SeekOrigin.Current);
                        decopmressed.Read(buffer, 0, 12);
                        var size = Convert.ToInt64(Encoding.UTF8.GetString(buffer, 0, 12).Trim('\0').Trim(), 8);

                        decopmressed.Seek(376L + size, SeekOrigin.Current);

                        var pos = decopmressed.Position;

                        var offset = 512 - (pos % 512);
                        if (offset == 512)
                            offset = 0;

                        decopmressed.Seek(offset, SeekOrigin.Current);
                    }
                    else
                    {
                        decopmressed.Seek(24, SeekOrigin.Current);
                        decopmressed.Read(buffer, 0, 12);
                        var size = Convert.ToInt64(Encoding.UTF8.GetString(buffer, 0, 12).Trim('\0').Trim(), 8);

                        decopmressed.Seek(376L, SeekOrigin.Current);

                        DbFile.Delete();
                        using var dbStream = DbFile.Create();
                        var buf = new byte[size];
                        decopmressed.Read(buf, 0, buf.Length);
                        dbStream.Write(buf, 0, buf.Length);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while downloading maxmind");
            }

            return false;
        }
    }
}