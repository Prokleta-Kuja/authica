using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MaxMind.GeoIP2;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace authica.Services
{
    public class IpSecurity
    {
        readonly FileInfo _dbFile;
        readonly ILogger<IpSecurity> _logger;
        readonly IHttpClientFactory _httpClientFactory;
        readonly IMemoryCache _cache;
        public IpSecurity(ILogger<IpSecurity> logger, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _cache = memoryCache;
            _dbFile = new(C.Paths.AppDataFor("GeoLite2-Country.mmdb"));
        }
        string GetBanKey(string ipAddress) => $"Banned_{ipAddress}";
        string GetTryKey(string ipAddress) => $"Try_{ipAddress}";
        public bool IsAllowed(string ipAddress)
        {
            if (_cache.TryGetValue(GetBanKey(ipAddress), out bool exists))
                return false;

            // Perform country check only if configured
            if (C.Configuration.Current.AllowedCountryCodes.Any())
            {
                if (!_dbFile.Exists)
                    return true;

                using var reader = new DatabaseReader(_dbFile.FullName);
                if (reader.TryCountry(ipAddress, out var response))
                    return C.Configuration.Current.AllowedCountryCodes.Contains(response?.Country?.IsoCode ?? string.Empty);
            }

            return false;
        }
        public void LogInfraction(string ipAddress)
        {
            var key = GetTryKey(ipAddress);
            var prevInfractions = _cache.GetOrCreate(key, entry =>
            {
                entry.SlidingExpiration = C.Configuration.Current.InfractionExpiration;
                return default(int);
            });

            var newInfractions = prevInfractions + 1;
            if (newInfractions >= C.Configuration.Current.MaxInfractions)
            {
                _cache.Remove(key);
                var entry = _cache.CreateEntry(GetBanKey(ipAddress));
                entry.SetValue(true);
                entry.SetAbsoluteExpiration(C.Configuration.Current.BanTime);
            }
            else
                _cache.Set<int>(key, newInfractions);
        }
        public void ConfigurationChanged()
        {
            // TODO: clear all keys after configuration has changed
        }
        public async ValueTask DownloadDb()
        {
            try
            {
                _logger.LogInformation("Downloading MaxMind db");

                var licenceKey = C.Configuration.Current.MaxMindLicenseKey;
                using var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"https://download.maxmind.com/app/geoip_download?edition_id=GeoLite2-Country&license_key={licenceKey}&suffix=tar.gz");

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

                    if (Path.GetFileName(name) != Path.GetFileName(_dbFile.FullName))
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

                        using var dbStream = _dbFile.Create();
                        var buf = new byte[size];
                        decopmressed.Read(buf, 0, buf.Length);
                        dbStream.Write(buf, 0, buf.Length);

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while downloading maxmind");
            }
        }
    }
}