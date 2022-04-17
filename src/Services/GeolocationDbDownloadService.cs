using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace authica.Services;

public class GeolocationDbDownloadService
{
    readonly ILogger<GeolocationDbDownloadService> _logger;
    readonly IHttpClientFactory _httpClientFactory;
    public GeolocationDbDownloadService(ILogger<GeolocationDbDownloadService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }
    public async ValueTask<bool> DownloadDb(string? licenseKeyOverride = null)
    {
        try
        {
            var licenceKey = licenseKeyOverride ?? C.Configuration.Current.MaxMindLicenseKey;

            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"https://download.maxmind.com/app/geoip_download?edition_id=GeoLite2-Country&license_key={licenceKey}&suffix=tar.gz");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Could not download Geolocation Db: {Reason}", response.ReasonPhrase);
                return false;
            }

            var lastModified = response.Content.Headers.LastModified;
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

                if (Path.GetFileName(name) != Path.GetFileName(C.GeoLocationDbFile.FullName))
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

                    C.GeoLocationDbFile.Delete();
                    using var dbStream = C.GeoLocationDbFile.Create();
                    var buf = new byte[size];
                    decopmressed.Read(buf, 0, buf.Length);
                    dbStream.Write(buf, 0, buf.Length);

                    if (lastModified.HasValue)
                        C.GeoLocationDbFile.CreationTimeUtc = lastModified.Value.UtcDateTime;

                    _logger.LogInformation("Geolocation Db downloaded");

                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while downloading MaxMind db");
        }

        return false;
    }
}