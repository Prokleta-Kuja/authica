using System;
using System.IO;
using System.Threading.Tasks;
using authica.Services;
using Microsoft.Extensions.Logging;
using Quartz;

namespace authica.Jobs
{
    public class GeolocationDbDownloadJob : IJob
    {
        readonly ILogger<GeolocationDbDownloadJob> _logger;
        readonly IpSecurity _ipSec;
        public GeolocationDbDownloadJob(ILogger<GeolocationDbDownloadJob> logger, IpSecurity ipSec)
        {
            _logger = logger;
            _ipSec = ipSec;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(C.Configuration.Current.MaxMindLicenseKey))
            {
                _logger.LogDebug("Cancelled {JobName}, no license key configured", nameof(GeolocationDbDownloadJob));
                return;
            }

            var dbFile = new FileInfo(C.Paths.AppDataFor(IpSecurity.DbFileName));
            if (dbFile.Exists && (DateTime.UtcNow - dbFile.CreationTimeUtc) < TimeSpan.FromHours(84)) // 3.5 days
            {
                _logger.LogDebug("Cancelled {JobName}, no need to update", nameof(GeolocationDbDownloadJob));
                return;
            }

            _logger.LogDebug("Executing {JobName}", nameof(GeolocationDbDownloadJob));

            if (await _ipSec.DownloadDb())
                _logger.LogInformation("MaxMind geolocation db updated");
        }
    }
}